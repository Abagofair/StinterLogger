using StinterLogger.RaceLogging.Iracing.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.IracingEventArgs
{
    public class DebugEventArgs : EventArgs
    {
        public DebugLog DebugLog { get; set; }
    }
}
