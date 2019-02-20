using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.IracingEventArgs
{
    public class DriverConnectionEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public int DriverSessionId { get; set; }
        public string DriverName { get; set; }
    }
}
