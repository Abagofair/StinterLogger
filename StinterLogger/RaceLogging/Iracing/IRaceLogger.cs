using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.RaceLogging.Iracing.Fuel;
using System;

namespace StinterLogger.RaceLogging.Iracing
{
    public interface IRaceLogger
    {
        #region events
        event EventHandler<LapCompletedEventArgs> LapCompleted;

        event EventHandler<RaceStateEventArgs> RaceStateChanged;

        event EventHandler PitRoad;

        event EventHandler<DriverConnectionEventArgs> Connected;

        event EventHandler Disconnected;
        #endregion

        #region methods
        void SetFuelLevelOnPitStop(int fuelLevel);

        void Start();

        void Stop();
        #endregion

        #region properties
        DriverInfo ActiveDriverInfo { get; }
        #endregion
    }
}
