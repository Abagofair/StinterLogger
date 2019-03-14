using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using StinterLogger.RaceLogging.Iracing.Debug;
using StinterLogger.RaceLogging.Iracing.SimEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using StinterLogger.RaceLogging.Iracing.Program.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Program
{
    public class ProgramLoader
    {
        private string _folderName;

        private string _programConfigsPath;

        private List<string> _programNames;

        private DebugLogger _debugLogger;

        public ProgramLoader(string folderName, DebugLogger debugLogger)
        {
            this._folderName = folderName;

            this._programConfigsPath = Directory.GetCurrentDirectory() + this._folderName;

            this._debugLogger = debugLogger;

            if (!Directory.Exists(this._programConfigsPath))
            {
                throw new DirectoryNotFoundException("ProgramConfigs doesn't exist");
            }
            else
            {
                this._programNames = GetProgramNames();               
            }
        }

        private List<string> GetProgramNames()
        {
            var programNames = new List<string>();

            try
            {
                foreach (var fileName in Directory.EnumerateFiles(this._programConfigsPath))
                {
                    var splitString = fileName.Split('\\', '.');
                    var parsedName = splitString[splitString.Length - 2];
                    programNames.Add(parsedName);
                }
                //this._debugLogger.CreateEventLog("Program names read");
            }
            catch (IOException e)
            {
                this._debugLogger.CreateExceptionLog("An IOException occurred when trying to read program names", e);
            }
            catch (UnauthorizedAccessException e)
            {
                this._debugLogger.CreateExceptionLog("The user didn't have access to this directory", e);
            }

            return programNames;
        }

        private string ReadJsonConfig(string path, string programName)
        {
            string json;
            try
            {   
                using (StreamReader sr = new StreamReader(path + "default.json"))
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

#pragma warning disable CS0618 // Type or member is obsolete

        private bool ValidatePropertyName(JsonToken token, object value, string propertyName)
        {
            return value != null && token == JsonToken.PropertyName && ((string)value).ToLower() == propertyName;
        }

        public ProgramConfig LoadLocalProgram(string programName)
        {
            if (!this._programNames.Contains(programName))
            {
                throw new ArgumentException("The requested program doesn't exist");
            }

            var jsonConfig = this.ReadJsonConfig(this._programConfigsPath, programName);
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
                //debug log
            }

            return programConfig;
        }
    }
#pragma warning disable CS0618 // Type or member is obsolete
}
