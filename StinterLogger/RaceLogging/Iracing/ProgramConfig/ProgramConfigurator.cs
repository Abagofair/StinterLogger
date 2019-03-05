using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using StinterLogger.RaceLogging.Iracing.Debug;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.ProgramConfig
{
    public class ProgramConfigurator
    {
        private const int RANDOM_MAX_LENGTH = 140;

        private const int LAPS_PER_STINT_DEFAULT = 1;

        private const int STINTCOUNT_DEFAULT = 1;

        private const float TELEMETRY_UPDATE_FREQUENCY_DEFAULT = 1.0f;

        private string _folderName;

        private string _programConfigsPath;

        private List<string> _programNames;

        private DebugLogger _debugLogger;

        public ProgramConfigurator(string folderName, DebugLogger debugLogger)
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
                this._debugLogger.CreateEventLog("Program names read");
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

        private ProgramConfig CleanProgramConfig(ProgramConfig programConfig)
        {
            var cleanProgramConfig = new ProgramConfig();

            cleanProgramConfig.TelemetryUpdateFrequency = programConfig.TelemetryUpdateFrequency;
            cleanProgramConfig.Name = programConfig.Name;
            cleanProgramConfig.StintCount = programConfig.StintCount;

            if (programConfig.TelemetryUpdateFrequency <= 0.0)
            {
                cleanProgramConfig.TelemetryUpdateFrequency = TELEMETRY_UPDATE_FREQUENCY_DEFAULT;
                this._debugLogger.CreateEventLog("TelemetryUpdateFrequency was lower than the allowed frequency. It has been set to " + TELEMETRY_UPDATE_FREQUENCY_DEFAULT + ".");
            }

            if (programConfig.Name.Length > RANDOM_MAX_LENGTH)
            {
                cleanProgramConfig.Name = programConfig.Name.Substring(0, RANDOM_MAX_LENGTH);
                this._debugLogger.CreateEventLog("Read the Name to be longer than " + RANDOM_MAX_LENGTH + ". Extra characters has been removed.");
            }

            if (programConfig.LapsPerStint <= 0)
            {
                cleanProgramConfig.LapsPerStint = LAPS_PER_STINT_DEFAULT;
                this._debugLogger.CreateEventLog("Read LapsPerStint <= 0. Has been reset to " + LAPS_PER_STINT_DEFAULT + ".");
            }

            if (programConfig.StintCount < 0)
            {
                cleanProgramConfig.StintCount = STINTCOUNT_DEFAULT;
                this._debugLogger.CreateEventLog("Read StintCount < 0. Has been reset to " + STINTCOUNT_DEFAULT + ".");
            }

            cleanProgramConfig.Data.Telemetry = VerifyData<Telemetry>(programConfig.Data.Telemetry);
            cleanProgramConfig.Data.Track = VerifyData<Track>(programConfig.Data.Track);
            cleanProgramConfig.Data.Time = VerifyData<Telemetry>(programConfig.Data.Time);

            return cleanProgramConfig;
        }

        private Dictionary<string, bool> VerifyData<T>(Dictionary<string, bool> dataConfig) where T : new()
        {
            var clearedData = new Dictionary<string, bool>();

            var dataType = (new T()).GetType();
            
            foreach (var dataProp in dataConfig.Keys)
            {
                if (dataConfig[dataProp] && dataType.GetProperty(dataProp) != null)
                {
                    clearedData.Add(dataProp, dataConfig[dataProp]);
                }
                else
                {
                    this._debugLogger.CreateEventLog("The data option couldn't be found or was set as false");
                }
            }

            return clearedData;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void JsonValidation(object sender, ValidationEventArgs eventArgs)
        {
            this._debugLogger.CreateExceptionLog("The JSON does not match the expected schema.", eventArgs.Exception);
            throw new JsonSerializationException(eventArgs.Message);
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

            JsonValidatingReader validatingReader = new JsonValidatingReader(reader)
            {
                Schema = ProgramJsonSchema.GetJsonSchema()
            };
            validatingReader.ValidationEventHandler += this.JsonValidation;

            JsonSerializer serializer = new JsonSerializer();
            var programConfig = serializer.Deserialize<ProgramConfig>(validatingReader);

            var cleanConfig = this.CleanProgramConfig(programConfig);

            return cleanConfig;
        }
    }
#pragma warning disable CS0618 // Type or member is obsolete
}
