using StinterLogger.RaceLogging.Iracing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing
{
    public interface IDataLogger<T>
    {
        event EventHandler<T> OnDataModelChange;

        void Enable();
        void Disable();

        IDataModel DataModel { get; set;  }
    }
}
