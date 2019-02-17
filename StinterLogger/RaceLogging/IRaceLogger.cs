using iRacingSdkWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
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
        int DriverId { get; }
        #endregion
    }
}
