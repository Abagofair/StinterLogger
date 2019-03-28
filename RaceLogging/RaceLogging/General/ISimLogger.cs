using RaceLogging.General.Entities;
using RaceLogging.General.SimEventArgs;
using System;

namespace RaceLogging
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

        event EventHandler<RaceStateEventArgs> RaceState;

        void SetTelemetryUpdateHz(double hz);

        void StartListening();

        void StopListening();

        void AddFuelOnPitStop(int fuelToAdd);

        Driver CurrentDriver { get; }

        Track CurrentTrack { get; }

        bool IsLive { get; }
    }
}
