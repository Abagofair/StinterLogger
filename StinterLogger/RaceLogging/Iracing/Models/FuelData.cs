using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Models
{
    public class FuelData : IDataModel
    {
        private const float LITERS_TO_GALLONS = 0.264172052f;

        //If units is false, data is needed in gallons.
        //The underlying representation is still metric.

        private float ConvertData(float value)
        {
            return Units ? value : value * LITERS_TO_GALLONS;
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

        public bool Units { get; set; }

        public Guid Guid => throw new NotImplementedException();
    }
}
