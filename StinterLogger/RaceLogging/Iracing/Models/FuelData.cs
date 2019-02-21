using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Models
{
    public class FuelData : IDataModel
    {
        public float FuelInTank { get; set; }

        public float TotalFuelUsed { get; set; }

        public float TotalRaceTime { get; set; }

        public float FuelUsagePerLap { get; set; }

        public float FuelToFinish { get; set; }

        public float RemainingRacetime { get; set; }

        public int LapsCompleted { get; set; }

        public int LapsRemaining { get; set; }

        public bool Units { get; set; }

        public Guid Guid => throw new NotImplementedException();
    }
}
