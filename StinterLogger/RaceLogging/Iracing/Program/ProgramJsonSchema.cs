using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.Program
{
    public class ProgramJsonSchema
    {
        private static JsonSchema _programSchema = null;

#pragma warning disable CS0618 // Type or member is obsolete
        public static JsonSchema GetJsonSchema()
        {
            if (ProgramJsonSchema._programSchema == null)
            {
                JsonSchema schema = new JsonSchema
                {
                    Type = JsonSchemaType.Object,
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        { "Name", new JsonSchema { Type = JsonSchemaType.String, Required = true } },
                        { "TelemetryUpdateFrequency", new JsonSchema { Type = JsonSchemaType.Float, Required = true } },
                        { "StintCount", new JsonSchema { Type = JsonSchemaType.Integer, Required = true } },
                        { "LapsPerStint", new JsonSchema { Type = JsonSchemaType.Integer, Required = true } },
                        {
                            "Data", new JsonSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, JsonSchema>
                                {
                                    { "Track", new JsonSchema { Type = JsonSchemaType.Object } },
                                    { "Telemetry", new JsonSchema { Type = JsonSchemaType.Object } },
                                    { "Time", new JsonSchema { Type = JsonSchemaType.Object } }
                                },
                                Required = true
                            }
                        }
                    },
                    Required = true,
                    AllowAdditionalProperties = false,
                    AllowAdditionalItems = false
                };

                ProgramJsonSchema._programSchema = schema;
            }

            return ProgramJsonSchema._programSchema;
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
