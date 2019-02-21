using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Models
{
    public class DriverInfo
    {
        public int GlobalUserId { get; set; }
        public int LocalId { get; set; }
        public string DriverName { get; set; }
        public bool Units { get; set; }
    }
}
