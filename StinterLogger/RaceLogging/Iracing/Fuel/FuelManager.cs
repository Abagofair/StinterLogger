using StinterLogger.RaceLogging;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.UI.FuelPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Fuel
{
    public class FuelManager : IDataLogger
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
            this.FuelModel = null;
            this._raceLogger.PitRoad += this.OnPitRoad;
            this._hasPitted = false;
        }

        #region events
        public event EventHandler FuelModelChange;
        #endregion

        #region event invocations
        private void OnFuelModelChange()
        {
            this.FuelModelChange?.Invoke(this, new EventArgs());
        }
        #endregion

        #region properties
        public FuelModel FuelModel { get; set; }

        public int GraceLaps { get; set; }
        #endregion

        #region public methods
        public void Enable()
        {
            //listen for green flag
            this.FuelModel = new FuelModel();
            this.FuelModel.Enabled = true;
            this.StartFuelLogging();
            this._raceLogger.RaceStateChanged += this.OnRaceStateChange;
        }

        public void Disable()
        {
            //restart model
            this.FuelModel.Enabled = false;
            this._raceLogger.RaceStateChanged -= this.OnRaceStateChange;
            this.FuelModel = null;
        }

        public void ResetFuelData()
        {
            this.FuelModel = new FuelModel();
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

            this.FuelModel.TotalLapTime += telemetryFromLastLap.LapTime;
            this.FuelModel.TotalUsed += firstReading.FuelLevel - lastReading.FuelLevel;
            this.FuelModel.LapsCompleted += 1;

            this.FuelModel.PerLap = this.FuelModel.TotalUsed / this.FuelModel.LapsCompleted;

            this.FuelModel.RemainingSessionTime = (float)telemetryFromLastLap.RemainingSessionTime;

            this.FuelModel.InTank = lastReading.FuelLevel;

            var remainingLaps = this.RemainingLaps((float)telemetryFromLastLap.RemainingSessionTime, this.FuelModel.TotalLapTime, this.FuelModel.LapsCompleted, this.GraceLaps);

            this.FuelModel.AmountToAdd = Math.Abs(this.FuelNeededToFinish(remainingLaps, this.FuelModel.PerLap));
            this.FuelModel.AmountToAdd = this.FuelModel.AmountToAdd > MAX_FUEL ? MAX_FUEL : this.FuelModel.AmountToAdd;

            this._hasPitted = false;

            this.OnFuelModelChange();
        }

        private void OnPitRoad(object sender, EventArgs eventArgs)
        {
            if (this.FuelModel != null && !this._hasPitted)
            {
                int cast = (int)this.FuelModel.AmountToAdd;
                float diff = this.FuelModel.AmountToAdd - (float)cast;
                if (diff >= 0.5f)
                {
                    this._raceLogger.SetFuelLevelOnPitStop((int)(this.FuelModel.AmountToAdd + 1));
                }
                else
                {
                    this._raceLogger.SetFuelLevelOnPitStop((int)this.FuelModel.AmountToAdd);
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
