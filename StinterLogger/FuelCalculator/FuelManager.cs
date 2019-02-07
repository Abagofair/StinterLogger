using StinterLogger.RaceLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*if (((App)Application.Current).SdkWrapper.GetTelemetryValue<bool>("OnPitRoad").Value && !_onPitRoad)
            {
                this._onPitRoad = true;
                if (this.FuelModel.AmountToAdd > 0)
                {
                    ((App)Application.Current).SdkWrapper.Chat.Activate();
                    ((App)Application.Current).SdkWrapper.Chat.Clear();
                    ((App)Application.Current).SdkWrapper.PitCommands.AddFuel((int)this.FuelModel.AmountToAdd);
                }
            }
            else if (!((App)Application.Current).SdkWrapper.GetTelemetryValue<bool>("OnPitRoad").Value)
            {
                this._onPitRoad = false;
            }*/

namespace StinterLogger.FuelCalculator
{
    public class FuelManager
    {
        private IRaceLogger _raceLogger;
        private bool _enabled;
        private RaceState _raceState;

        public FuelModel FuelModel { get; set; }

        public int GraceLaps { get; set; }

        public FuelManager(IRaceLogger raceLogger, int graceLaps)
        {
            this._raceLogger = raceLogger;
            this._enabled = false;
            this._raceState = RaceState.UNKNOWN;
            this.GraceLaps = graceLaps;
        }

        public event EventHandler FuelModelChange;

        public void Enable()
        {
            //listen for green flag
            this._enabled = true;
            this._raceLogger.RaceStateChanged += this.OnRaceStateChange;
        }

        public void Disable()
        {
            //restart model
            this._enabled = false;
            this._raceLogger.RaceStateChanged -= this.OnRaceStateChange;
        }

        public void ResetFuelData()
        {
            this.FuelModel = new FuelModel();
        }

        private void StartFuelLogging()
        {
            this.ResetFuelData();
            this._raceLogger.LapCompleted += OnLapCompleted;
        }

        private void StopFuelLogging()
        {
            this._raceLogger.LapCompleted -= OnLapCompleted;
        }

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
            this.FuelModel.PerLap = firstReading.FuelLevel - lastReading.FuelLevel;
            this.FuelModel.LapsCompleted += 1;

            var remainingLaps = this.RemainingLaps((float)telemetryFromLastLap.RemainingSessionTime, this.FuelModel.TotalLapTime, this.FuelModel.LapsCompleted, this.GraceLaps);
            this.FuelModel.AmountToAdd = this.FuelNeededToFinish(remainingLaps, this.FuelModel.PerLap);

            //Invoke fuelmodelchange
            this.OnFuelModelChange();
        }

        private void OnFuelModelChange()
        {
            this.FuelModelChange.Invoke(this, new EventArgs());
        }

        private float FuelNeededToFinish(float remainingRaceTime, float timeRaced, float lapsCompleted, float additionalLaps, float fuelUsedPerLap)
        {
            if (fuelUsedPerLap < 0.0f)
            {
                throw new ArgumentException();
            }

            return RemainingLaps(remainingRaceTime, timeRaced, lapsCompleted, additionalLaps) * fuelUsedPerLap;
        }

        private float FuelNeededToFinish(float remainingLaps, float fuelUsedPerLap)
        {
            if (fuelUsedPerLap < 0.0f)
            {
                throw new ArgumentException();
            }

            return remainingLaps * fuelUsedPerLap;
        }

        private float RemainingLaps(float remainingRaceTime, float timeRaced, float lapsCompleted, float additionalLaps)
        {
            if (remainingRaceTime < 0.0f || timeRaced < 0.0f || additionalLaps < 0.0f || lapsCompleted < 0.0f)
            {
                throw new ArgumentException();
            }
            float averageLapTime = timeRaced / lapsCompleted;
            return (remainingRaceTime / averageLapTime) + additionalLaps;
        }
    }
}
