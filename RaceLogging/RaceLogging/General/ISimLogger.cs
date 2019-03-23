using RaceLogging.General.Entities;
using StinterLogger.RaceLogging.General.SimEventArgs;
using System;

namespace StinterLogger.RaceLogging
{
    public interface ISimLogger
    {
        event EventHandler<TelemetryEventArgs> TelemetryRecieved;

        event EventHandler<LapCompletedEventArgs> LapCompleted;

        event EventHandler<DriverConnectionEventArgs> DriverConnected;

        event EventHandler<DriverConnectionEventArgs> DriverDisconnected;

        event EventHandler<TrackLocationEventArgs> TrackLocationChange;

        event EventHandler<TireEventArgs> TireWearUpdated;

        event EventHandler<PitRoadEventArgs> PitRoad;

        void SetTelemetryUpdateHz(double hz);

        void StartListening();

        void StopListening();

        void AddFuelOnPitStop(int fuelToAdd);

        Driver CurrentDriver { get; }

        Track CurrentTrack { get; }

        bool IsLive { get; }
    }
}
