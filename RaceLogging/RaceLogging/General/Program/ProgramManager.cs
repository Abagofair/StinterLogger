using RaceLogging.General.SimEventArgs;
using RaceLogging.General.Program.Config;

using RaceLogging.General.Entities;
using RaceLogging.General.Enums;

using System;
using System.Timers;

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

        private SimProgram _programData;

        private Lap _currentData;

        private Timer _timer;

        public ProgramManager(ISimLogger simLogger, IProgramLoader programLoader)
        {
            this._simLogger = simLogger;
            this._programLoader = programLoader;
        }

        public event EventHandler<ProgramEndEventArgs> ProgramEnd;
        private void OnProgramEnd(ProgramEndEventArgs e)
        {
            this.ProgramEnd?.Invoke(this, e);
        }

        public void Load(string path, string fileName)
        {
            try
            {
                this._currentProgramConfig = this._programLoader.LoadProgram(path, fileName);
            }
            catch (ProgramLoaderException)
            {
                this._currentProgramConfig = null;
            }
        }

        public void StartProgram()
        {
            if (this._currentProgramConfig == null)
            {
                throw new ProgramManagerException("There is not a program config loaded");
            }
            else
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
            }

            if (this._currentProgramConfig.EndCondition.Condition == Condition.Minutes)
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
                this._simLogger.PitRoad += this.OnPitRoad;
            }

            this._simLogger.LapCompleted += this.OnLapCompleted;
        }

        public void Stop()
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
                this._simLogger.PitRoad -= this.OnPitRoad;
            }

            this._simLogger.LapCompleted -= this.OnLapCompleted;

            if (this._currentProgramConfig.EndCondition.Condition == Condition.Minutes)
            {
                this._timer.Elapsed -= this.OnConditionMinutesMet;
                this._timer.Stop();
            }

            this._endConditionCurrentCount = 0;

            this.OnProgramEnd(new ProgramEndEventArgs
            {
                SimProgram = this._programData
            });
        }

        private bool IsProgramComplete()
        {
            return this._endConditionCurrentCount == this._currentProgramConfig.EndCondition.Count;
        }

        private void OnLapCompleted(object sender, LapCompletedEventArgs eventArgs)
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

            if (this._currentProgramConfig.EndCondition.Condition == Condition.Laps)
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

        private void OnTelemetryRecieved(object sender, TelemetryEventArgs eventArgs)
        {
            this._currentData.Telemetry.Add(eventArgs.Telemetry);
        }

        private void OnTireData(object sender, TireEventArgs eventArgs)
        {
            if (!this._tireDataRecieved && this._currentData.Pit != null)
            {
                this._currentData.Pit.Tire = eventArgs.Tires;
                this._tireDataRecieved = true;
            }
        }

        private void OnTrackLocation(object sender, TrackLocationEventArgs eventArgs)
        {
            if (eventArgs.TrackLocation == TrackLocation.InPitStall)
            {
                if (this._currentProgramConfig.EndCondition.Condition == Condition.InPitStall)
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

        private void OnPitRoad(object sender, PitRoadEventArgs eventArgs)
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

        private void OnConditionMinutesMet(object sender, ElapsedEventArgs eventArgs)
        {
            this.Stop();
        }
    }
}
