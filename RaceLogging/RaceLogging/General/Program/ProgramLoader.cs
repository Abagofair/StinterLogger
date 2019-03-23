using Newtonsoft.Json;
using RaceLogging.General.Entities;
using RaceLogging.General.Debug;
using RaceLogging.General.Program.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace RaceLogging.General.Program
{
    public class ProgramLoader : IProgramLoader
    {
        private readonly DebugManager _debugLogger;

        public ProgramLoader()
        {}

        public ProgramLoader(DebugManager debugLogger)
        {
            this._debugLogger = debugLogger;
        }

        private string ReadJsonConfig(string path, string programName)
        {
            string json;
            try
            {   
                using (StreamReader sr = new StreamReader(Path.Combine(path, programName)))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                json = null;
                this._debugLogger.CreateExceptionLog("The program could not be read", e);
            }
            return json;
        }

        private List<string> VerifyData<T>(List<string> dataConfig) where T : new()
        {
            var clearedData = new List<string>();

            var dataType = (new T()).GetType();
            
            foreach (var dataProp in dataConfig)
            {
                if (dataType.GetProperty(dataProp, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) != null)
                {
                    clearedData.Add(dataProp);
                }
                else
                {
                    //this._debugLogger.CreateEventLog("The data option couldn't be found or was set as false");
                }
            }

            return clearedData;
        }

        private bool VerifyFilePath(string path, string fileName)
        {
            bool directoryExist = Directory.Exists(path);
            var abs = Path.Combine(path, fileName);
            if (directoryExist)
            {
                foreach (var name in Directory.EnumerateFiles(path))
                {
                    if (name == abs)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
#pragma warning disable CS0618 // Type or member is obsolete
        private bool ValidatePropertyName(JsonToken token, object value, string propertyName)
        {
            return value != null && token == JsonToken.PropertyName && ((string)value).ToLower() == propertyName;
        }

        public ProgramConfig LoadProgram(string path, string fileName)
        {
            if (!this.VerifyFilePath(path, fileName))
            {
                throw new ArgumentException("The requested program could not be found");
            }

            var jsonConfig = this.ReadJsonConfig(path, fileName);
            if (jsonConfig == null)
            {
                throw new IOException("The program could not be read");
            }

            JsonTextReader reader = new JsonTextReader(new StringReader(jsonConfig));

            var programConfig = new ProgramConfig();
            try
            {
                while (reader.Read())
                {
                    if (ValidatePropertyName(reader.TokenType, reader.Value, "name"))
                    {
                        reader.Read();
                        programConfig.Name =
                            reader.Value != null && reader.TokenType == JsonToken.String
                            ? (string)reader.Value : "";
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "telemetryupdatefrequency"))
                    {
                        reader.Read();
                        programConfig.TelemetryUpdateFrequency =
                            reader.Value != null && reader.TokenType == JsonToken.Float
                            ? (double)reader.Value : 4.0f;
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "endcondition"))
                    {
                        reader.Read();
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            reader.Read();
                            var condition =
                                reader.Value != null && reader.TokenType == JsonToken.String
                            ? ((string)reader.Value).ToLower() : "freeroam";

                            bool pitstall = condition == "inpitstall";
                            bool laps = condition == "laps";
                            bool minutes = condition == "minutes";

                            if (pitstall || laps || minutes)
                            {
                                reader.Read();
                                var count =
                                    reader.Value != null && reader.TokenType == JsonToken.Integer
                                ? ((long)reader.Value) : 1;

                                if (pitstall)
                                {
                                    programConfig.EndCondition.Condition = Condition.InPitStall;
                                }
                                else if (laps)
                                {
                                    programConfig.EndCondition.Condition = Condition.Laps;
                                }
                                else if (minutes)
                                {
                                    programConfig.EndCondition.Condition = Condition.Minutes;
                                }
                                programConfig.EndCondition.Count = (int)count;
                            }
                            else
                            {
                                programConfig.EndCondition.Condition = Condition.FreeRoam;
                            }
                        }
                        else
                        {
                            programConfig.EndCondition.Condition = Condition.FreeRoam;
                        }
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "logpitdelta"))
                    {
                        reader.Read();
                        programConfig.LogPitDelta =
                            reader.Value != null && reader.TokenType == JsonToken.Boolean
                            ? (bool)reader.Value : false;
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "logtirewear"))
                    {
                        reader.Read();
                        programConfig.LogTireWear =
                            reader.Value != null && reader.TokenType == JsonToken.Boolean
                            ? (bool)reader.Value : false;
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "telemetry"))
                    {
                        reader.Read();
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                var telemetry =
                                   reader.Value != null && reader.TokenType == JsonToken.String
                                   ? ((string)reader.Value).ToLower() : "";

                                programConfig.Telemetry.Add(telemetry);
                            }

                            programConfig.Telemetry = VerifyData<Telemetry>(programConfig.Telemetry);

                            continue;
                        }
                    }
                }
            }
            catch (JsonException e)
            {
                throw new ProgramLoaderException(e.Message);
            }

            return programConfig;
        }
    }
#pragma warning disable CS0618 // Type or member is obsolete
}
