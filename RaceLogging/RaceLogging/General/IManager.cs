using StinterLogger.RaceLogging.General.Models;
using System;

namespace StinterLogger.RaceLogging.General
{
    public interface IManager<T> where T : EventArgs
    {
        event EventHandler<T> OnDataModelChange;

        void Enable();
        void Disable();

        IDataModel DataModel { get; set; }
    }
}
