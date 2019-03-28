using Newtonsoft.Json;
using RaceLogging.General.Entities;
using RaceLogging.General.Debug;
using RaceLogging.General.Program.Config;
using System;
using System.Collections.Generic;
using System.IO;
using RaceLogging.General.Program;

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
                var requiredValues = new List<string>(ProgramConfig.RequiredConfigValues);
                while (reader.Read())
                {
                    if (ValidatePropertyName(reader.TokenType, reader.Value, "name"))
                    {
                        reader.Read();
                        if (reader.Value == null || !(reader.Value is string))
                        {
                            throw new ProgramLoaderException("The Name value is missing or is not a string");
                        }
                        else
                        {
                            programConfig.Name = ((string)reader.Value).ToLower();
                            requiredValues.Remove("name");
                        }
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "telemetryupdatefrequency"))
                    {
                        reader.Read();
                        if (reader.Value == null || !(reader.Value is double))
                        {
                            throw new ProgramLoaderException("The TelemetryUpdateFrequency value is missing or is not a double");
                        }
                        else
                        {
                            programConfig.TelemetryUpdateFrequency = ((double)reader.Value);
                            requiredValues.Remove("telemetryupdatefrequency");
                        }
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "startcondition"))
                    {
                        reader.Read();
                        if (reader.Value == null || !(reader.Value is string))
                        {
                            throw new ProgramLoaderException("The startcondition value is missing or is invalid");
                        }
                        else
                        {
                            var startCondition = ((string)reader.Value).ToLower();
                            if (startCondition == StartConditionValues.AfterOutLap.ToString().ToLower())
                            {
                                programConfig.StartCondition = StartConditionValues.AfterOutLap;
                            }
                            else if (startCondition == StartConditionValues.GreenFlag.ToString().ToLower())
                            {
                                programConfig.StartCondition = StartConditionValues.GreenFlag;
                            }
                            else if (startCondition == StartConditionValues.PitExit.ToString().ToLower())
                            {
                                programConfig.StartCondition = StartConditionValues.PitExit;
                            }
                            else if (startCondition == StartConditionValues.None.ToString().ToLower())
                            {
                                programConfig.StartCondition = StartConditionValues.None;
                            }
                            else
                            {
                                throw new ProgramLoaderException(startCondition + " is not a valid startcondition");
                            }
                            requiredValues.Remove("startcondition");
                        }
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "endcondition"))
                    {
                        string invalidCondition = "The endcondition value is missing or is invalid";

                        reader.Read();
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            reader.Read();
                            string condition;
                            if (reader.Value == null || !(reader.Value is string))
                            {
                                throw new ProgramLoaderException(invalidCondition);
                            }
                            else
                            {
                                condition = ((string)reader.Value).ToLower();
                            }

                            bool pitstall = condition == EndConditionValues.InPitStall.ToString().ToLower();
                            bool laps = condition == EndConditionValues.Laps.ToString().ToLower();
                            bool minutes = condition == EndConditionValues.Minutes.ToString().ToLower();
                            bool freeroam = condition == EndConditionValues.FreeRoam.ToString().ToLower();

                            if (pitstall || laps || minutes)
                            {
                                reader.Read();
                                if (reader.Value == null || !(reader.Value is long))
                                {
                                    throw new ProgramLoaderException("Endcondition is missing an integer count");
                                }
                                else
                                {
                                    programConfig.EndCondition.Count = (Convert.ToInt32(reader.Value));
                                }

                                if (pitstall)
                                {
                                    programConfig.EndCondition.Condition = EndConditionValues.InPitStall;
                                }
                                else if (laps)
                                {
                                    programConfig.EndCondition.Condition = EndConditionValues.Laps;
                                }
                                else if (minutes)
                                {
                                    programConfig.EndCondition.Condition = EndConditionValues.Minutes;
                                }
                            }
                            else if (freeroam)
                            {
                                programConfig.EndCondition.Condition = EndConditionValues.FreeRoam;
                            }
                            else
                            {
                                throw new ProgramLoaderException(condition + " is not a valid endcondition");
                            }
                        }
                        else
                        {
                            throw new ProgramLoaderException(invalidCondition);
                        }

                        requiredValues.Remove("endcondition");
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "logpitdelta"))
                    {
                        reader.Read();
                        if (reader.Value == null || !(reader.Value is bool))
                        {
                            throw new ProgramLoaderException("The LogPitDelta value is missing or is not a boolean");
                        }
                        else
                        {
                            programConfig.LogPitDelta = ((bool)reader.Value);
                            requiredValues.Remove("logpitdelta");
                        }
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "logtirewear"))
                    {
                        reader.Read();
                        if (reader.Value == null || !(reader.Value is bool))
                        {
                            throw new ProgramLoaderException("The LogTireWear value is missing or is not a boolean");
                        }
                        else
                        {
                            programConfig.LogTireWear = ((bool)reader.Value);
                            requiredValues.Remove("logtirewear");
                        }
                        continue;
                    }
                    else if (ValidatePropertyName(reader.TokenType, reader.Value, "telemetry"))
                    {
                        reader.Read();

                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.Value == null || !(reader.Value is string))
                                {
                                    throw new ProgramLoaderException("The LogTireWear value is missing or is not a boolean");
                                }
                                else
                                {
                                    programConfig.Telemetry.Add(((string)reader.Value).ToLower());
                                }
                            }

                            programConfig.Telemetry = VerifyData<Telemetry>(programConfig.Telemetry);
                            requiredValues.Remove("telemetry");
                            continue;
                        }
                        else
                        {
                            throw new ProgramLoaderException("The expected value of the telemetry property is an array");
                        }
                    }
                }

                if (requiredValues.Count != 0)
                {
                    //build message with missing values
                    string error = "These config values are missing: ";
                    foreach (var value in requiredValues)
                    {
                        error += value + ", ";
                    }
                    throw new ProgramLoaderException(error);
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
