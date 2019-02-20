using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing
{
    public interface IDataLogger
    {
        void Enable();
        void Disable();
    }
}
