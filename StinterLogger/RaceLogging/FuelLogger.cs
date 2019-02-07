//"WRAPPED"
using iRacingSdkWrapper;
using StinterLogger.FuelCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
{
    public class FuelLogger : IDataLogger
    {
        private IRaceLogger _raceLogger;

        public FuelLogger(IRaceLogger raceLogger)
        {
            this._raceLogger = raceLogger;
        }

        public void Disable()
        {
            this._raceLogger.UnregisterTelemetryListener(this.TelemetryListener);
        }

        public void Enable()
        {
            this._raceLogger.RegisterTelemetryListener(this.TelemetryListener);
        }

        private void TelemetryListener(object sender, SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {

        }
    }
}
