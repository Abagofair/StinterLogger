using StinterLogger.RaceLogging;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.RaceLogging.Iracing.Fuel;
using StinterLogger.UI.FuelPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StinterLogger.RaceLogging.Iracing.Debug;
using System.Windows;
using StinterLogger.UI.MainApp;

namespace StinterLogger.RaceLogging.Iracing.Fuel
{
    public class FuelManager : IDataLogger<FuelDataEventArgs>
    {
        #region private constants
        private const float MAX_FUEL = 999.0f;
        #endregion

        #region fields
        private IRaceLogger _raceLogger;

        private DebugLogger _debugLogger;

        private bool _outLap;
        #endregion

        public FuelManager(IRaceLogger raceLogger, int graceLaps)
        {
            this._raceLogger = raceLogger;

            this.FuelData = null;

            this._raceLogger.PitRoad += this.OnPitRoad;

            this._outLap = false;

            this._debugLogger = ((App)Application.Current).DebugLogger;
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
        private FuelManagerData FuelData { get; set; }

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
            this.StartFuelManager();
            this._debugLogger.CreateEventLog("Started the fuel manager");
            //this._raceLogger.RaceStateChanged += this.OnRaceStateChange;
        }

        public void Disable()
        {
            //restart model
            this.StopFuelManager();
            this._debugLogger.CreateEventLog("Stopped the fuel manager");
            //this._raceLogger.RaceStateChanged -= this.OnRaceStateChange;
            this.FuelData = null;
        }

        public void ResetFuelData()
        {
            this.FuelData = new FuelManagerData();
            this.FuelData.Unit = this._raceLogger.ActiveDriverInfo.Unit;
        }

        public void SetGraceValue(float value)
        {
            this.FuelData.GraceOption.Value = value;
        }
        #endregion

        #region private methods
        private void StartFuelManager()
        {
            this.ResetFuelData();
            this._raceLogger.LapCompleted += OnLapCompleted;
        }

        private void StopFuelManager()
        {
            this._raceLogger.LapCompleted -= OnLapCompleted;
        }
        #endregion

        #region listeners
        private void OnRaceStateChange(object sender, RaceStateEventArgs raceStateEventArgs)
        {
            if (raceStateEventArgs.RaceState == RaceState.GREEN)
            {
                this.StartFuelManager();
            }
            else
            {
                this.StopFuelManager();
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

            var remainingLaps = this.RemainingLaps((float)telemetryFromLastLap.RemainingSessionTime, this.FuelData.TotalRaceTime, this.FuelData.LapsCompleted);

            this.FuelData.LapsRemaining = (int)remainingLaps;

            this.FuelData.FuelToFinish = this.FuelNeededToFinish(remainingLaps, this.FuelData.FuelUsagePerLap, this.FuelData.FuelInTank);
            this.FuelData.FuelToFinish = this.FuelData.FuelToFinish > MAX_FUEL ? MAX_FUEL : this.FuelData.FuelToFinish;

            this._outLap = false;

            this.OnFuelModelChange(new FuelDataEventArgs
            {
                FuelData = this.FuelData
            });
        }

        private void OnPitRoad(object sender, EventArgs eventArgs)
        {
            if (this.FuelData != null && !this._outLap)
            {
                this._debugLogger.CreateEventLog("On pit road");

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
                this._outLap = true;
            }
        }
        #endregion

        #region fuel calculation
        private float GraceFuel(float exactFuelNeeded, float graceValue, GraceMode graceMode, float FuelPerLap)
        {
            float extraFuel = 0.0f;
            if (graceMode == GraceMode.Lap)
            {
                extraFuel = graceValue * FuelPerLap;
            }
            else if (graceMode == GraceMode.Percent)
            {
                extraFuel = exactFuelNeeded * (graceValue / 100.0f);
            }

            return extraFuel;
        }

        private float FuelNeededToFinish(float remainingRaceTime, float timeRaced, float lapsCompleted, float fuelUsedPerLap)
        {
            return RemainingLaps(remainingRaceTime, timeRaced, lapsCompleted) * fuelUsedPerLap;
        }

        private float FuelNeededToFinish(float remainingLaps, float fuelUsedPerLap, float fuelInTank)
        {
            float exactValue = remainingLaps * fuelUsedPerLap;

            this._debugLogger.CreateDataLog("Exact fuel needed: " + exactValue);

            float extraFuel = GraceFuel(exactValue, this.FuelData.GraceOption.Value, this.FuelData.GraceOption.Mode, this.FuelData.FuelUsagePerLap);

            this._debugLogger.CreateDataLog("Extra fuel needed: " + extraFuel);

            float fuelNeeded = (exactValue + extraFuel) - fuelInTank;

            this._debugLogger.CreateDataLog("Actual fuel needed: " + fuelNeeded);

            return fuelNeeded > 0.0f ? fuelNeeded : 0.0f;
        }

        private float RemainingLaps(float remainingRaceTime, float timeRaced, float lapsCompleted)
        {
            float averageLapTime = timeRaced / lapsCompleted;

            this._debugLogger.CreateDataLog("Average lap time: " + averageLapTime);

            float remainingLaps = remainingRaceTime / averageLapTime;

            this._debugLogger.CreateDataLog("Remaining laps: " + remainingLaps);

            return remainingLaps;
        }
        #endregion
    }
}
