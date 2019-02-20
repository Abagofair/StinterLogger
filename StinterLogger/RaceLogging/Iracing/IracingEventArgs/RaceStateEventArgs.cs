using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.IracingEventArgs
{
    public class RaceStateEventArgs : EventArgs
    {
        public RaceState RaceState { get; set; }
    }
}
