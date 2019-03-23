using RaceLogging.General.Entities;
using System;

namespace RaceLogging.General.Fuel
{
    public class FuelManagerData : IEntity
    {
        private const float LITERS_TO_GALLONS = 0.264172052f;

        //If units is false, data is needed in gallons.
        //The underlying representation is still metric.

        public FuelManagerData()
        {
            this.GraceOption = new GraceOption
            {
                Mode = GraceMode.Lap,
                Value = 1.0f
            };
        }

        private float ConvertData(float value)
        {
            var convertedValue = value;
            if (this.Unit == FuelUnit.Gallons)
            {
                convertedValue *= LITERS_TO_GALLONS;
            }
            return convertedValue;
        }

        private float _fuelInTank;
        public float FuelInTank
        {
            get
            {
                return ConvertData(this._fuelInTank);
            }

            set => this._fuelInTank = value;
        }

        private float _totalFuelUsed;
        public float TotalFuelUsed
        {
            get
            {
                return ConvertData(this._totalFuelUsed);
            }

            set => this._totalFuelUsed = value;
        }

        public float TotalRaceTime { get; set; }

        private float _fuelUsagePerLap;
        public float FuelUsagePerLap
        {
            get
            {
                return ConvertData(this._fuelUsagePerLap);
            }

            set => this._fuelUsagePerLap = value;
        }

        private float _fuelToFinish;
        public float FuelToFinish
        {
            get
            {
                return ConvertData(this._fuelToFinish);
            }

            set => this._fuelToFinish = value;
        }

        public float RemainingRacetime { get; set; }

        public int LapsCompleted { get; set; }

        public int LapsRemaining { get; set; }
        
        public FuelUnit Unit { get; set; }

        public GraceOption GraceOption { get; set; }

        public Guid Guid => throw new NotImplementedException();
    }
}
