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
        void OnTelemetryUpdatedHandler(object sender, SdkWrapper.TelemetryUpdatedEventArgs);

        //event OnTelemetryUpdatedHandler OnTelemetryUpdate;
    }
}
