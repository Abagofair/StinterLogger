using StinterLogger.RaceLogging;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using StinterLogger.UI.FuelPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Fuel
{
    public class FuelManager : IDataLogger<FuelDataEventArgs>
    {
        #region private constants
        private const float MAX_FUEL = 999.0f;
        #endregion

        #region fields
        private IRaceLogger _raceLogger;

        private bool _hasPitted;
        #endregion

        public FuelManager(IRaceLogger raceLogger, int graceLaps)
        {
            this._raceLogger = raceLogger;
            this.GraceLaps = graceLaps;
            this.FuelData = null;
            this._raceLogger.PitRoad += this.OnPitRoad;
            this._hasPitted = false;
        }

        #region events
        public event EventHandler<FuelDataEventArgs> OnDataModelChange;
        #endregion

        #region event invocations
        private void OnFuelModelChange(FuelDataEventArgs fuelDataEventArgs)
        {
            this.OnDataModelChange?.Invoke(this, fuelDataEventArgs);
        }
        #endregion

        #region properties
        private FuelData FuelData { get; set; }

        public int GraceLaps { get; set; }

        public IDataModel DataModel
        {
            get => this.FuelData;
            set => throw new NotImplementedException();
        }
        #endregion

        #region public methods
        public void Enable()
        {
            //listen for green flag
            ResetFuelData();
            this.StartFuelLogging();
            this._raceLogger.RaceStateChanged += this.OnRaceStateChange;
        }

        public void Disable()
        {
            //restart model
            this._raceLogger.RaceStateChanged -= this.OnRaceStateChange;
            this.FuelData = null;
        }

        public void ResetFuelData()
        {
            this.FuelData = new FuelData();
            this.FuelData.Units = this._raceLogger.ActiveDriverInfo.Units;
        }
        #endregion

        #region private methods
        private void StartFuelLogging()
        {
            this.ResetFuelData();

            this._raceLogger.LapCompleted += OnLapCompleted;
        }

        private void StopFuelLogging()
        {
            this._raceLogger.LapCompleted -= OnLapCompleted;
        }
        #endregion

        #region listeners
        private void OnRaceStateChange(object sender, RaceStateEventArgs raceStateEventArgs)
        {
            if (raceStateEventArgs.RaceState == RaceState.GREEN)
            {
                this.StartFuelLogging();
            }
            else
            {
                this.StopFuelLogging();
            }
        }

        private void OnLapCompleted(object sender, LapCompletedEventArgs lapCompletedEventArgs)
        {
            var telemetryFromLastLap = lapCompletedEventArgs.TelemetryLapData;

            var firstReading = telemetryFromLastLap.Readings[0];
            var lastReading = telemetryFromLastLap.Readings[telemetryFromLastLap.Readings.Count - 1];

            this.FuelData.TotalRaceTime += telemetryFromLastLap.LapTime;
            this.FuelData.TotalFuelUsed += firstReading.FuelLevel - lastReading.FuelLevel;
            this.FuelData.LapsCompleted += 1;

            this.FuelData.FuelUsagePerLap = this.FuelData.TotalFuelUsed / this.FuelData.LapsCompleted;

            this.FuelData.RemainingRacetime = (float)telemetryFromLastLap.RemainingSessionTime;

            this.FuelData.FuelInTank = lastReading.FuelLevel;

            var remainingLaps = this.RemainingLaps((float)telemetryFromLastLap.RemainingSessionTime, this.FuelData.TotalRaceTime, this.FuelData.LapsCompleted, this.GraceLaps);

            this.FuelData.LapsRemaining = (int)remainingLaps;

            this.FuelData.FuelToFinish = Math.Abs(this.FuelNeededToFinish(remainingLaps, this.FuelData.TotalFuelUsed));
            this.FuelData.FuelToFinish = this.FuelData.FuelToFinish > MAX_FUEL ? MAX_FUEL : this.FuelData.FuelToFinish;

            this._hasPitted = false;

            this.OnFuelModelChange(new FuelDataEventArgs
            {
                FuelData = this.FuelData
            });
        }

        private void OnPitRoad(object sender, EventArgs eventArgs)
        {
            if (this.FuelData != null && !this._hasPitted)
            {
                int cast = (int)this.FuelData.FuelToFinish;
                float diff = this.FuelData.FuelToFinish - (float)cast;
                if (diff >= 0.5f)
                {
                    this._raceLogger.SetFuelLevelOnPitStop((int)(this.FuelData.FuelToFinish + 1));
                }
                else
                {
                    this._raceLogger.SetFuelLevelOnPitStop((int)this.FuelData.FuelToFinish);
                }
                this._hasPitted = true;
            }
        }
        #endregion

        #region fuel calculation
        private float FuelNeededToFinish(float remainingRaceTime, float timeRaced, float lapsCompleted, float additionalLaps, float fuelUsedPerLap)
        {
            return RemainingLaps(remainingRaceTime, timeRaced, lapsCompleted, additionalLaps) * fuelUsedPerLap;
        }

        private float FuelNeededToFinish(float remainingLaps, float fuelUsedPerLap)
        {
            return remainingLaps * fuelUsedPerLap;
        }

        private float RemainingLaps(float remainingRaceTime, float timeRaced, float lapsCompleted, float additionalLaps)
        {
            float averageLapTime = timeRaced / lapsCompleted;
            return (remainingRaceTime / averageLapTime) + additionalLaps;
        }
        #endregion
    }
}
