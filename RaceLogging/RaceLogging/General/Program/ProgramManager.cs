using RaceLogging.General.SimEventArgs;
using RaceLogging.General.Program.Config;

using RaceLogging.General.Entities;
using RaceLogging.General.Enums;

using System;
using System.Timers;
using RaceLogging.RaceLogging.General.Entities;

namespace RaceLogging.General.Program
{
    public class ProgramManager
    {
        private readonly ISimLogger _simLogger;

        private readonly IProgramLoader _programLoader;

        private ProgramConfig _currentProgramConfig;

        private int _endConditionCurrentCount;

        private Timing.Timer _pitDeltaTimer;

        private bool _waitingForPitDelta;

        private bool _tireDataRecieved;

        private bool _isProgramActive;

        private bool _isInitialized;

        private bool _delayedStartActivated;

        private SimProgram _programData;

        private Lap _currentData;

        private Timer _timer;

        public ProgramManager(ISimLogger simLogger, IProgramLoader programLoader)
        {
            this._simLogger = simLogger;
            this._programLoader = programLoader;
        }

        public bool IsProgramActive { get => _isProgramActive; }

        public event EventHandler<ProgramEndEventArgs> ProgramEnd;
        private void OnProgramEnd(ProgramEndEventArgs e)
        {
            this.ProgramEnd?.Invoke(this, e);
        }

        public event EventHandler ProgramActivated;
        private void OnProgramStart()
        {
            this.ProgramActivated?.Invoke(this, EventArgs.Empty);
        }

        public void Load(string path, string fileName)
        {
            try
            {
                this._currentProgramConfig = this._programLoader.LoadProgram(path, fileName);
                if (!this._simLogger.IsLive)
                {
                    this._simLogger.DriverConnected += this.OnConnected;
                }
                else
                {
                    this.Start();
                }
            }
            catch (ProgramLoaderException e)
            {
                this._currentProgramConfig = null;
                throw new ProgramLoaderException("Error while loading the program: " + e.Message);
            }
        }

        public void Start()
        {
            if (this._currentProgramConfig == null)
            {
                throw new ProgramManagerException("There is not a program config loaded");
            }
            else if (!this._isInitialized)
            {
                this.Initialize();
                this._isInitialized = true;
            }

            if (this._currentProgramConfig.StartCondition == StartConditionValues.None)
            {
                this.ApplyListeners();
            }
            else if (this._delayedStartActivated)
            {
                //wait on delayed start condition
                this.ApplyListeners();
            }
        }

        private void OnConnected(object sender, EventArgs eventArgs)
        {
            this.Start();
            this._simLogger.DriverConnected -= this.OnConnected;
        }

        private void Initialize()
        {
            if (!this._simLogger.IsLive)
            {
                this._simLogger.StartListening();
            }

            this._simLogger.SetTelemetryUpdateHz(this._currentProgramConfig.TelemetryUpdateFrequency);
            this._tireDataRecieved = false;
            this._pitDeltaTimer = new Timing.Timer();
            this._programData = new SimProgram
            {
                Driver = this._simLogger.CurrentDriver,
                ProgramConfig = this._currentProgramConfig
            };
            this._currentData = new Lap();
            this._endConditionCurrentCount = 0;

            //delayed start listeners
            if (this._currentProgramConfig.StartCondition == StartConditionValues.GreenFlag)
            {
                this._simLogger.RaceState += this.OnRaceStateChange;
            }
            else if (this._currentProgramConfig.StartCondition == StartConditionValues.AfterOutLap)
            {
                this._simLogger.LapCompleted += this.OnLapCompleted;
            }
            else if (this._currentProgramConfig.StartCondition == StartConditionValues.PitExit)
            {
                this._simLogger.PitRoad += this.OnPitRoad;
            }
        }

        private void ApplyListeners()
        {
            if (this._currentProgramConfig.EndCondition.Condition == EndConditionValues.Minutes)
            {
                var timeInMs = this._currentProgramConfig.EndCondition.Count * 60000;
                if (timeInMs > int.MaxValue || timeInMs < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    this._timer = new Timer(timeInMs);
                    this._timer.Elapsed += this.OnConditionMinutesMet;
                    this._timer.AutoReset = false;
                    this._timer.Enabled = true;
                }
            }
            else if (this._currentProgramConfig.EndCondition.Condition == EndConditionValues.Checkered)
            {
                this._simLogger.RaceState -= this.OnRaceStateChange;
                this._simLogger.RaceState += this.OnRaceStateChange;
            }

            if (this._currentProgramConfig.Telemetry.Count > 0)
            {
                this._simLogger.TelemetryRecieved += this.OnTelemetryRecieved;
            }

            if (this._currentProgramConfig.LogTireWear)
            {
                this._simLogger.TireWearUpdated += this.OnTireData;
            }

            if (this._currentProgramConfig.LogPitDelta)
            {
                this._simLogger.TrackLocationChange += this.OnTrackLocation;
            }

            this._simLogger.PitRoad -= this.OnPitRoad;
            this._simLogger.PitRoad += this.OnPitRoad;

            this._simLogger.LapCompleted -= this.OnLapCompleted;
            this._simLogger.LapCompleted += this.OnLapCompleted;

            this._isProgramActive = true;
            this.OnProgramStart();

            this._delayedStartActivated = false;
        }

        public void Stop()
        {
            if (this._isProgramActive)
            {
                if (this._currentProgramConfig.Telemetry.Count > 0)
                {
                    this._simLogger.TelemetryRecieved -= this.OnTelemetryRecieved;
                }

                if (this._currentProgramConfig.LogTireWear)
                {
                    this._simLogger.TireWearUpdated -= this.OnTireData;
                }

                if (this._currentProgramConfig.LogPitDelta)
                {
                    this._simLogger.TrackLocationChange -= this.OnTrackLocation;
                }

                this._simLogger.PitRoad -= this.OnPitRoad;
                this._simLogger.LapCompleted -= this.OnLapCompleted;

                if (this._currentProgramConfig.EndCondition.Condition == EndConditionValues.Minutes)
                {
                    this._timer.Elapsed -= this.OnConditionMinutesMet;
                    this._timer.Stop();
                }

                this._endConditionCurrentCount = 0;

                this._programData.ProgramDebrief = this.CreateProgramDebrief(this._programData);

                this.OnProgramEnd(new ProgramEndEventArgs
                {
                    SimProgram = this._programData
                });

                this._isProgramActive = false;
            }
        }

        private bool IsProgramComplete()
        {
            return this._endConditionCurrentCount == this._currentProgramConfig.EndCondition.Count;
        }

        private void OnRaceStateChange(object sender, RaceStateEventArgs eventArgs)
        {
            if (!this._isProgramActive && this._currentProgramConfig.StartCondition == StartConditionValues.GreenFlag && eventArgs.RaceState == RaceLogging.General.Enums.RaceState.GreenFlag)
            {
                this._delayedStartActivated = true;
                this.Start();
                return;
            }
            else if (this._isProgramActive && this._currentProgramConfig.EndCondition.Condition == EndConditionValues.Checkered && eventArgs.RaceState == RaceLogging.General.Enums.RaceState.Checkered)
            {
                this.Stop();
                return;
            }
        }

        private void OnLapCompleted(object sender, LapCompletedEventArgs eventArgs)
        {
            if (this._isProgramActive)
            {
                var telemetry = this._currentData.Telemetry;
                Pit pit = null;
                if (this._currentData.Pit != null)
                {
                    pit = this._currentData.Pit;
                }
                this._currentData = eventArgs.Lap;
                this._currentData.Pit = pit;
                this._currentData.Telemetry = telemetry;
                this._programData.CompletedLaps.Add(this._currentData);
                this._currentData.LapNumber = this._programData.CompletedLaps.Count;

                if (this._currentProgramConfig.EndCondition.Condition == EndConditionValues.Laps)
                {
                    if (this.IsProgramComplete())
                    {
                        this.Stop();
                        return;
                    }

                    ++this._endConditionCurrentCount;
                }

                this._currentData = new Lap();
                if (this._waitingForPitDelta)
                {
                    this._currentData.Pit = new Pit();
                }
            }
            else if (!this._isProgramActive && this._currentProgramConfig.StartCondition == StartConditionValues.AfterOutLap)
            {
                this._delayedStartActivated = true;
                this.Start();
                return;
            }
        }

        private void OnTelemetryRecieved(object sender, TelemetryEventArgs eventArgs)
        {
            if (this._isProgramActive)
            {
                this._currentData.Telemetry.Add(eventArgs.Telemetry);
            }
        }

        private void OnTireData(object sender, TireEventArgs eventArgs)
        {
            if (this._isProgramActive && !this._tireDataRecieved && this._currentData.Pit != null)
            {
                this._currentData.Pit.Tire = eventArgs.Tires;
                this._tireDataRecieved = true;
            }
        }

        private void OnTrackLocation(object sender, TrackLocationEventArgs eventArgs)
        {
            if (this._isProgramActive)
            {
                if (eventArgs.TrackLocation == TrackLocation.InPitStall)
                {
                    if (this._currentProgramConfig.EndCondition.Condition == EndConditionValues.InPitStall)
                    {
                        if (this.IsProgramComplete())
                        {
                            this.Stop();
                            return;
                        }

                        ++this._endConditionCurrentCount;
                    }

                    if (this._currentData.Pit != null)
                    {
                        this._currentData.Pit.WasInStall = true;
                    }
                }
            }
        }

        private void OnPitRoad(object sender, PitRoadEventArgs eventArgs)
        {
            if (this._isProgramActive)
            {
                if (eventArgs.IsOnPitRoad)
                {
                    this._pitDeltaTimer.StartTimer();
                    if (!this._waitingForPitDelta)
                    {
                        this._currentData.Pit = new Pit();
                        this._waitingForPitDelta = true;
                    }
                }
                else
                {
                    double endTimer = this._pitDeltaTimer.StopTimer();
                    if (this._currentData.Pit != null && this._waitingForPitDelta && endTimer > 0.0)
                    {
                        this._currentData.Pit.PitDeltaSeconds = endTimer;
                        this._waitingForPitDelta = false;
                    }

                    this._tireDataRecieved = false;
                }
            }
            else if (!this._isProgramActive && this._currentProgramConfig.StartCondition == StartConditionValues.PitExit && !eventArgs.IsOnPitRoad)
            {
                this._delayedStartActivated = true;
                this.Start();
                return;
            }
        }

        private void OnConditionMinutesMet(object sender, ElapsedEventArgs eventArgs)
        {
            this.Stop();
        }

        private ProgramDebrief CreateProgramDebrief(SimProgram program)
        {
            if (program.CompletedLaps.Count <= 0)
            {
                return new ProgramDebrief(0);
            }

            var debrief = new ProgramDebrief(program.CompletedLaps[0].Time.SectorTimes.Count);

            int tireWearCount = 0;

            foreach (var laps in program.CompletedLaps)
            {
                debrief.AvgLapTime += laps.Time.LapTime;
                debrief.AvgFuelUsagePrLap += laps.FuelUsed;
                
                for (int i = 0; i < debrief.AvgSectorTimes.Length; ++i)
                {
                    debrief.AvgSectorTimes[i] += laps.Time.SectorTimes[i];
                }

                if (laps.Pit != null && laps.Pit.Tire != null)
                {
                    var tires = laps.Pit.Tire;
                    debrief.AvgLFWearPrLap += ((tires.LFwearL + tires.LFwearM + tires.LFwearR) / 3);
                    debrief.AvgLRWearPrLap += ((tires.LRwearL + tires.LRwearM + tires.LRwearR) / 3);
                    debrief.AvgRFWearPrLap += ((tires.RFwearL + tires.RFwearM + tires.RFwearR) / 3);
                    debrief.AvgRRWearPrLap += ((tires.RRwearL + tires.RRwearM + tires.RRwearR) / 3);

                    ++tireWearCount;

                    if (laps.Pit.WasInStall && debrief.MaxPitDeltaWithStall < laps.Pit.PitDeltaSeconds)
                    {
                        debrief.MaxPitDeltaWithStall = laps.Pit.PitDeltaSeconds;
                    }
                }
            }

            debrief.AvgLapTime /= program.CompletedLaps.Count;
            debrief.AvgFuelUsagePrLap /= program.CompletedLaps.Count;
            for (int i = 0; i < debrief.AvgSectorTimes.Length; ++i)
            {
                debrief.AvgSectorTimes[i] /= program.CompletedLaps.Count;
            }

            debrief.AvgLFWearPrLap /= tireWearCount;
            debrief.AvgLRWearPrLap /= tireWearCount;
            debrief.AvgRFWearPrLap /= tireWearCount;
            debrief.AvgRRWearPrLap /= tireWearCount;

            return debrief;
        }
    }
}
