using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Fuel
{
    public class DriverInfo
    {
        public int GlobalUserId { get; set; }
        public int LocalId { get; set; }
        public string DriverName { get; set; }
        public FuelUnit Unit { get; set; }
    }
}
