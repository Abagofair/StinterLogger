using StinterLogger.RaceLogging.Iracing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.IracingEventArgs
{
    public class DriverConnectionEventArgs : EventArgs
    {
        public DriverInfo ActiveDriverInfo { get; set; }
    }
}
