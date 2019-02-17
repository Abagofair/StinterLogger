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
        event EventHandler<LapCompletedEventArgs> LapCompleted;

        event EventHandler<RaceStateEventArgs> RaceStateChanged;

        void SetFuelLevelOnPitStop(int fuelLevel);

        void Start();

        void Stop();
    }
}
