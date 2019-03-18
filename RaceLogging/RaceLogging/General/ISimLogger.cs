using StinterLogger.RaceLogging.General.Models;
using StinterLogger.RaceLogging.General.SimEventArgs;
using System;

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

        event EventHandler<TrackLocationEventArgs> TrackLocationChange;

        event EventHandler<TireEventArgs> TireWearUpdated;

        event EventHandler<PitRoadEventArgs> PitRoad;

        void SetTelemetryUpdateHz(double hz);

        void StartListening();

        void StopListening();

        void SetFuelLevelOnPitStop(int fuelToAdd);

        Driver ActiveDriverInfo { get; }

        Track TrackInfo { get; }

        bool IsLive { get; set; }
    }
}
