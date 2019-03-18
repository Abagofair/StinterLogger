using StinterLogger.RaceLogging.General.Debug;
using StinterLogger.RaceLogging.General.Models;
using StinterLogger.RaceLogging.General.SimEventArgs;
using System;

namespace StinterLogger.RaceLogging.General.Fuel
{
    public class FuelManager : IManager<FuelDataEventArgs>
    {
        #region private constants
        private const float MAX_FUEL = 999.0f;
        #endregion

        #region fields
        private readonly ISimLogger _simLogger;

        private readonly DebugManager _debugLogger;

        private bool _outLap;
        #endregion

        public FuelManager(ISimLogger simLogger, DebugManager debugManager, int graceLaps)
        {
            this._simLogger = simLogger;

            this._debugLogger = debugManager;

            this.FuelData = null;

            this._simLogger.PitRoad += this.OnPitRoad;

            this._outLap = false;
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
        }

        public void Disable()
        {
            //restart model
            this.StopFuelManager();
            this._debugLogger.CreateEventLog("Stopped the fuel manager");
            this.FuelData = null;
        }

        public void ResetFuelData()
        {
            this.FuelData = new FuelManagerData
            {
                Unit = this._simLogger.ActiveDriverInfo.Unit
            };
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
            this._simLogger.LapCompleted += this.OnLapCompleted;
        }

        private void StopFuelManager()
        {
            this._simLogger.LapCompleted -= this.OnLapCompleted;
        }
        #endregion

        #region listeners
        private void OnLapCompleted(object sender, LapCompletedEventArgs lapCompletedEventArgs)
        {
            var telemetryFromLastLap = lapCompletedEventArgs.Lap;

            this.FuelData.TotalRaceTime += telemetryFromLastLap.LapTime;
            this.FuelData.TotalFuelUsed += (lapCompletedEventArgs.Lap.FuelInTankAtStart - lapCompletedEventArgs.Lap.FuelInTankAtFinish);
            this.FuelData.LapsCompleted += 1;

            this.FuelData.FuelUsagePerLap = this.FuelData.TotalFuelUsed / this.FuelData.LapsCompleted;

            this.FuelData.RemainingRacetime = (float)telemetryFromLastLap.RemainingSessionTime;

            this.FuelData.FuelInTank = lapCompletedEventArgs.Lap.FuelInTankAtStart;

            var remainingLaps = this.RemainingLaps((float)telemetryFromLastLap.RemainingSessionTime, this.FuelData.TotalRaceTime, this.FuelData.LapsCompleted);

            this.FuelData.LapsRemaining = (int)remainingLaps;

            this.FuelData.FuelToFinish = this.CalculateFuelNeededToFinish(this.FuelData);

            this._outLap = false;

            this.OnFuelModelChange(new FuelDataEventArgs
            {
                FuelData = this.FuelData
            });
        }

        private void OnPitRoad(object sender, PitRoadEventArgs eventArgs)
        {
            if (eventArgs.IsOnPitRoad && this.FuelData != null && !this._outLap)
            {
                try
                {
                    int cast = (int)this.FuelData.FuelToFinish;
                    float diff = this.FuelData.FuelToFinish - (float)cast;
                    if (diff >= 0.5f)
                    {
                        this._simLogger.SetFuelLevelOnPitStop((int)(this.FuelData.FuelToFinish + 1));
                    }
                    else
                    {
                        this._simLogger.SetFuelLevelOnPitStop((int)this.FuelData.FuelToFinish);
                    }
                    this._outLap = true;
                }
                catch (NotImplementedException e)
                {
                    this._debugLogger.CreateExceptionLog("SetFuelLevelOnPitStop is not implemented", e);
                }
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

        private float LapsReachableOnCurrentTank(float fuelUsedPerLap, float fuelInTank)
        {
            return fuelUsedPerLap * fuelInTank;
        }

        private float LapsRemainingAfterTankUsed(float lapsRemainingInTank, float remainingLaps)
        {
            var amountOfLapsToFuel = remainingLaps - lapsRemainingInTank;
            return amountOfLapsToFuel >= 0.0f ? amountOfLapsToFuel : 0.0f;
        }

        private float FuelToAdd(float amountOfLapsToFuel, float fuelUsedPerLap)
        {
            return amountOfLapsToFuel * fuelUsedPerLap;
        }

        private float CalculateFuelNeededToFinish(FuelManagerData fuelData)
        {
            var lapsReachable = this.LapsReachableOnCurrentTank(fuelData.FuelUsagePerLap, fuelData.FuelInTank);

            this._debugLogger.CreateDataLog("Laps reachable on current tank amount: " + lapsReachable);

            var lapsRemainingAfterTank = this.LapsRemainingAfterTankUsed(lapsReachable, fuelData.LapsRemaining);

            this._debugLogger.CreateDataLog("Laps remaining after tank is used up: " + lapsRemainingAfterTank);

            var fuelToAdd = this.FuelToAdd(lapsRemainingAfterTank, fuelData.FuelUsagePerLap);

            this._debugLogger.CreateDataLog("Exact fuel amount to finish: " + fuelToAdd);
            //add grace fuel
            var graceFuel = this.GraceFuel(fuelToAdd, fuelData.GraceOption.Value, fuelData.GraceOption.Mode, fuelData.FuelUsagePerLap);

            this._debugLogger.CreateDataLog("Specified amount of grace fuel to add: " + graceFuel);
            this._debugLogger.CreateDataLog("Total amount of fuel to add: " + (fuelToAdd + graceFuel));

            return fuelToAdd + graceFuel;
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
