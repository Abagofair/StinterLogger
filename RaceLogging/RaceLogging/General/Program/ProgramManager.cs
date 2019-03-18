using StinterLogger.RaceLogging.General.Models;
using StinterLogger.RaceLogging.General.Program.Config;
using StinterLogger.RaceLogging.General.Program.Data;
using StinterLogger.RaceLogging.General.SimEventArgs;
using System;
using System.Timers;

namespace StinterLogger.RaceLogging.General.Program
{
    public class ProgramManager
    {
        private ISimLogger _simLogger;

        private IProgramLoader _programLoader;

        private ProgramConfig _currentProgramConfig;

        private int _endConditionCurrentCount;

        private RaceLogging.Timing.Timer _pitDeltaTimer;

        private bool _waitingForPitDelta;

        private bool _tireDataRecieved;

        private ProgramData _programData;

        private LapTelemetry _currentData;

        private Timer _timer;

        public ProgramManager(ISimLogger simLogger, IProgramLoader programLoader)
        {
            this._simLogger = simLogger;
            this._programLoader = programLoader;
        }

        public void Load(string programName)
        {
            try
            {
                this._currentProgramConfig = this._programLoader.LoadLocalProgram(programName);
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
                this._programData = new ProgramData();
                this._currentData = new LapTelemetry();
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

            //save data

        }

        private bool IsProgramComplete()
        {
            return this._endConditionCurrentCount == this._currentProgramConfig.EndCondition.Count;
        }

        private void OnLapCompleted(object sender, LapCompletedEventArgs eventArgs)
        {
            this._currentData.CompletedLap = eventArgs.Lap;
            this._programData.LapData.Add(this._currentData);

            if (this._currentProgramConfig.EndCondition.Condition == Condition.Laps)
            {
                if (this.IsProgramComplete())
                {
                    this.Stop();
                    return;
                }

                ++this._endConditionCurrentCount;
            }

            this._currentData = new LapTelemetry();
        }

        private void OnTelemetryRecieved(object sender, TelemetryEventArgs eventArgs)
        {
            this._currentData.Telemetries.Add(eventArgs.Telemetry);
        }

        private void OnTireData(object sender, TireEventArgs eventArgs)
        {
            if (!this._tireDataRecieved)
            {
                this._programData.Tires.Add(eventArgs.TireWear);
                this._tireDataRecieved = true;
            }
        }

        private void OnTrackLocation(object sender, TrackLocationEventArgs eventArgs)
        {
            if (eventArgs.TrackLocation == Models.TrackLocation.InPitStall)
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

                if (this._programData.PitDeltas.Count > 0)
                {
                    this._programData.PitDeltas[this._programData.PitDeltas.Count - 1].WasInStall = true;
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
                    this._programData.PitDeltas.Add(new Pit());
                    this._waitingForPitDelta = true;
                }
            }
            else
            {
                double endTimer = this._pitDeltaTimer.StopTimer();
                if (this._programData.PitDeltas.Count > 0 && this._waitingForPitDelta && endTimer > 0.0)
                {
                    this._programData.PitDeltas[this._programData.PitDeltas.Count - 1].PitDeltaSeconds = endTimer;
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
