using RaceLogging.General.Entities;
using System.Collections.Generic;

namespace RaceLogging.General.Program.Config
{
    public enum EndConditionValue
    {
        InPitStall, Minutes, Laps, FreeRoam
    }

    public class EndCondition
    {
        public EndConditionValue Condition { get; set; }
        public int Count { get; set; }
    }

    public class ProgramConfig
    {
        public ProgramConfig()
        {
            this.Telemetry = new List<string>();
            this.EndCondition = new EndCondition();
        }

        public static List<string> RequiredConfigValues => new List<string>
        {
            "name",
            "telemetryupdatefrequency",
            "logpitdelta",
            "logtirewear",
            "endcondition",
            "telemetry"
        };

        public string Name { get; set; }

        public double TelemetryUpdateFrequency { get; set; }

        public bool LogPitDelta { get; set; }

        public bool LogTireWear { get; set; }

        public EndCondition EndCondition { get; }

        public List<string> Telemetry { get; set; }

        public static List<Dictionary<string, object>> GetTelemetryValuesIfInConfig(List<string> telemetryConfig, List<Telemetry> telemetries)
        {
            var dict = new Dictionary<string, object>();
            var telemetryList = new List<Dictionary<string, object>>();
            foreach (var telemetry in telemetries)
            {
                Telemetry tm = new Telemetry();
                foreach (var telemetryProp in telemetry.GetType().GetProperties())
                {
                    if (telemetryConfig.Contains(telemetryProp.Name.ToLower()))
                    {
                        dict.Add(telemetryProp.Name, telemetryProp.GetValue(telemetry));
                    }
                }
                telemetryList.Add(dict);
                dict = new Dictionary<string, object>();
            }
            return telemetryList;
        }
    }
}
