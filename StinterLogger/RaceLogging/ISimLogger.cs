using StinterLogger.RaceLogging.Iracing.SimEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using StinterLogger.RaceLogging.Iracing.SimEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
{
    public interface ISimLogger
    {
        event EventHandler<TelemetryEventArgs> TelemetryRecieved;

        event EventHandler<DriverStateEventArgs> DriverStateChanged;

        event EventHandler RaceStateChanged;

        event EventHandler<LapCompletedEventArgs> LapCompleted;

        event EventHandler<DriverConnectionEventArgs> DriverConnected;

        event EventHandler<DriverConnectionEventArgs> DriverDisconnected;

        void StartListening();

        void StopListening();

        DriverInfo ActiveDriverInfo { get; }
    }
}
