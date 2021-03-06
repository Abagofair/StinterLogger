﻿using RaceLogging.General.Entities;
using RaceLogging.General.Debug;
using RaceLogging.General.SimEventArgs;
using System;

namespace RaceLogging.General.Fuel
{
    public class FuelManager
    {
        #region private constants
        private const float MAX_FUEL = 999.0f;

        private const int LAPS_RUNNING_AVG = 5;
        #endregion

        #region fields
        private readonly ISimLogger _simLogger;

        private readonly DebugManager _debugLogger;

        private int _lapCount;

        private float _totalFuelUsed;

        private float _fuelToAdd;

        private float _timeRemainingOfSession;

        private float _totalTimeRaced;

        private bool _active;

        private bool _hasPitted;

        private GraceOption _graceOption;
        #endregion

        public FuelManager(ISimLogger simLogger, DebugManager debugManager)
        {
            this._simLogger = simLogger;

            this._debugLogger = debugManager;

            this._simLogger.PitRoad += this.OnPitRoad;

            this._graceOption = new GraceOption
            {
                Mode = GraceMode.Lap,
                Value = 2.0f
            };

            this._hasPitted = false;
        }

        private void ResetData()
        {
            this._totalFuelUsed = 0.0f;
            this._lapCount = 0;
            this._fuelToAdd = 0.0f;
            this._totalTimeRaced = 0.0f;
            this._fuelToAdd = 0.0f;
        }

        #region public methods
        public void Enable()
        {
            //listen for green flag
            this.StartFuelManager();
            this._debugLogger.CreateEventLog("Started the fuel manager");
        }

        public void Disable()
        {
            //restart model
            this.StopFuelManager();
            this._debugLogger.CreateEventLog("Stopped the fuel manager");
        }

        public void SetGrace(GraceOption graceOption)
        {
            this._graceOption = graceOption;
        }
        #endregion

        #region private methods
        private void StartFuelManager()
        {
            this.ResetData();
            this._simLogger.RaceState += this.OnRaceStateChange;
        }

        private void StopFuelManager()
        {
            this.ResetData();
            this._active = false;
            this._simLogger.RaceState -= this.OnRaceStateChange;
            this._simLogger.LapCompleted -= this.OnLapCompleted;
        }
        #endregion

        #region listeners
        private void OnRaceStateChange(object sender, RaceStateEventArgs eventArgs)
        {
            if (eventArgs.RaceState == RaceLogging.General.Enums.RaceState.GreenFlag && !this._active)
            {
                this._simLogger.LapCompleted += this.OnLapCompleted;
                this._simLogger.PitRoad += this.OnPitRoad;
                this._active = true;
            }
            else if (eventArgs.RaceState == RaceLogging.General.Enums.RaceState.Checkered && this._active)
            {
                this._simLogger.LapCompleted -= this.OnLapCompleted;
                this._simLogger.PitRoad -= this.OnPitRoad;
                this._simLogger.RaceState -= this.OnRaceStateChange;
                this._active = false;
            }
        }

        private void OnLapCompleted(object sender, LapCompletedEventArgs lapCompletedEventArgs)
        {
            var lapData = lapCompletedEventArgs.Lap;

            if (this._lapCount >= LAPS_RUNNING_AVG)
            {
                this.ResetData();
            }

            ++this._lapCount;

            if (lapData.FuelUsed > 0.0f)
            {
                this._totalFuelUsed += lapData.FuelUsed;
            }

            if (lapData.RemainingSessionTime > 0.0f)
            {
                this._timeRemainingOfSession = lapData.RemainingSessionTime;
            }

            if (lapData.Time.LapTime > 0)
            {
                this._totalTimeRaced += lapData.Time.LapTime;
            }

            //calculate fuel needed to finish
            float avgLapTime = this._totalTimeRaced / this._lapCount;
            float lapsRemaining = this._timeRemainingOfSession / avgLapTime;
            float avgFuelPrLap = this._totalFuelUsed / this._lapCount;
            float totalFuelNeededToFinish = lapsRemaining * avgFuelPrLap;
            float fuelNeededToFinish = Math.Abs(totalFuelNeededToFinish - lapData.FuelInTankAtFinish);

            if (this._graceOption != null)
            {
                if (this._graceOption.Mode == GraceMode.Lap)
                {
                    fuelNeededToFinish += this._graceOption.Value * avgFuelPrLap;
                }
            }

            this._fuelToAdd = fuelNeededToFinish;
        }

        private void OnPitRoad(object sender, PitRoadEventArgs eventArgs)
        {
            if (eventArgs.IsOnPitRoad)
            {
                try
                {
                    if (this._fuelToAdd > 0 && this._fuelToAdd < MAX_FUEL && !this._hasPitted)
                    {
                        this._simLogger.AddFuelOnPitStop(Convert.ToInt32(this._fuelToAdd));
                        this._hasPitted = true;
                    }
                }
                catch (NotImplementedException e)
                {
                    this._debugLogger.CreateExceptionLog("SetFuelLevelOnPitStop is not implemented", e);
                }
            }
            else
            {
                this._hasPitted = false;
            }
        }
        #endregion
    }
}
