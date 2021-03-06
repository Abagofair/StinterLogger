﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaceLogging.General.Entities;
using RaceLogging.General.Program.Config;

namespace RaceLogging.General.Program
{
    public class ProgramWriter : IProgramWriter
    {
        public Task<byte[]> WriteProgramToByteArrayAsync(SimProgram program, CompressionLevel compressionLevel)
        {
            return Task.Factory.StartNew(() => this.WriteProgramToByteArray(program, compressionLevel), CancellationToken.None);
        }

        public Task<string> WriteProgramToStringAsync(SimProgram program)
        {
            return Task.Factory.StartNew(() => this.WriteProgramToString(program), CancellationToken.None);
        }

        public byte[] WriteProgramToByteArray(SimProgram simProgram, CompressionLevel compressionLevel)
        {
            var jsonBytes = this.WriteProgramToByteArray(simProgram);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, compressionLevel))
            {
                dstream.Write(jsonBytes, 0, jsonBytes.Length);
            }
            return output.ToArray();
        }

        private byte[] WriteProgramToByteArray(SimProgram simProgram)
        {
            var json = this.WriteProgramToString(simProgram);
            return Encoding.Unicode.GetBytes(json);
        }

        public string WriteProgramToString(SimProgram program)
        {
            var sw = new StringWriter();
            var writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            //write config
            writer.WritePropertyName("ProgramConfig");
            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(program.ProgramConfig.Name);

            writer.WritePropertyName("TelemetryUpdateFrequency");
            writer.WriteValue(program.ProgramConfig.TelemetryUpdateFrequency);

            writer.WritePropertyName("StartCondition");
            writer.WriteValue(program.ProgramConfig.StartCondition.ToString());

            writer.WritePropertyName("EndCondition");
            writer.WriteStartArray();
            writer.WriteValue(program.ProgramConfig.EndCondition.Condition.ToString());
            writer.WriteValue(program.ProgramConfig.EndCondition.Count);
            writer.WriteEndArray();

            writer.WritePropertyName("LogPitDelta");
            writer.WriteValue(program.ProgramConfig.LogPitDelta);

            writer.WritePropertyName("LogTireWear");
            writer.WriteValue(program.ProgramConfig.LogTireWear);

            writer.WritePropertyName("Telemetry");
            writer.WriteStartArray();
            foreach (var telemetry in program.ProgramConfig.Telemetry)
            {
                writer.WriteValue(telemetry);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
            //config written

            //write debrief
            writer.WritePropertyName("ProgramDebrief");
            writer.WriteStartObject();

            writer.WritePropertyName("AvgLapTime");
            writer.WriteValue(program.ProgramDebrief.AvgLapTime);

            writer.WritePropertyName("AvgSectorTimes");
            writer.WriteStartArray();
            foreach (var sector in program.ProgramDebrief.AvgSectorTimes)
            {
                writer.WriteValue(sector);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("AvgLFWearPrLap");
            writer.WriteValue(program.ProgramDebrief.AvgLFWearPrLap);
            writer.WritePropertyName("AvgRFWearPrLap");
            writer.WriteValue(program.ProgramDebrief.AvgRFWearPrLap);
            writer.WritePropertyName("AvgLRWearPrLap");
            writer.WriteValue(program.ProgramDebrief.AvgLRWearPrLap);
            writer.WritePropertyName("AvgRRWearPrLap");
            writer.WriteValue(program.ProgramDebrief.AvgRRWearPrLap);

            writer.WritePropertyName("AvgFuelUsagePrLap");
            writer.WriteValue(program.ProgramDebrief.AvgFuelUsagePrLap);

            writer.WritePropertyName("MaxPitDeltaWithStall");
            writer.WriteValue(program.ProgramDebrief.MaxPitDeltaWithStall);

            writer.WriteEndObject();
            //debrief written

            //data
            writer.WritePropertyName("ProgramData");

            writer.WriteStartObject();

            writer.WritePropertyName("SetupName");
            writer.WriteValue(program.SetupName);

            writer.WritePropertyName("Driver");

            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(program.Driver.DriverName);

            writer.WritePropertyName("Id");
            writer.WriteValue(program.Driver.GlobalUserId);

            writer.WriteEndObject();

            if (program.CompletedLaps.Count > 0)
            {
                writer.WritePropertyName("Track");
                //track object
                writer.WriteStartObject();
                var track = program.CompletedLaps[0].Track;
                writer.WritePropertyName("Name");
                writer.WriteValue(track.Name);
                writer.WritePropertyName("DisplayName");
                writer.WriteValue(track.DisplayName);
                writer.WritePropertyName("Length");
                writer.WriteValue(track.Length);
                writer.WritePropertyName("Sectors");

                writer.WriteStartArray();
                foreach (var sector in track.Sectors)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Number");
                    writer.WriteValue(sector.Number);
                    writer.WritePropertyName("MetersUntilSectorStarts");
                    writer.WriteValue(sector.MetersUntilSectorStarts);
                    writer.WritePropertyName("PctOfTrack");
                    writer.WriteValue(sector.PctOfTrack);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                //end of track object
                writer.WriteEndObject();

                var car = program.CompletedLaps[0].Car;
                writer.WritePropertyName("Car");
                writer.WriteValue(car.Name);

                writer.WritePropertyName("CompletedLaps");
                writer.WriteStartArray();
                foreach (var lap in program.CompletedLaps)
                {
                    //lap object
                    writer.WriteStartObject();
                    writer.WritePropertyName("LapNumber");
                    writer.WriteValue(lap.LapNumber);

                    writer.WritePropertyName("Time");
                    //time object
                    writer.WriteStartObject();
                    writer.WritePropertyName("Lap");
                    writer.WriteValue(lap.Time.LapTime);
                    writer.WritePropertyName("Sectors");
                    writer.WriteStartArray();
                    foreach (var sector in lap.Time.SectorTimes)
                    {
                        writer.WriteValue(sector);
                    }
                    writer.WriteEndArray();
                    //end of time object
                    writer.WriteEndObject();

                    writer.WritePropertyName("FuelAtStart");
                    writer.WriteValue(lap.FuelInTankAtStart);
                    writer.WritePropertyName("FuelAtFinish");
                    writer.WriteValue(lap.FuelInTankAtFinish);
                    writer.WritePropertyName("FuelUsed");
                    writer.WriteValue(lap.FuelUsed);

                    writer.WritePropertyName("Incidents");
                    writer.WriteValue(lap.Incidents);

                    writer.WritePropertyName("AirTemp");
                    writer.WriteValue(lap.AirTemp);
                    writer.WritePropertyName("TrackTemp");
                    writer.WriteValue(lap.TrackTemp);

                    if (program.ProgramConfig.LogPitDelta && lap.Pit != null)
                    {
                        writer.WritePropertyName("PitDelta");
                        writer.WriteValue(lap.Pit.PitDeltaSeconds);
                        writer.WritePropertyName("WasInStall");
                        writer.WriteValue(lap.Pit.WasInStall);
                    }

                    if (program.ProgramConfig.LogTireWear && lap.Pit != null && lap.Pit.Tire != null)
                    {
                        writer.WritePropertyName("Tires");
                        //tire object
                        writer.WriteStartObject();
                        writer.WritePropertyName("LFwearL");
                        writer.WriteValue(lap.Pit.Tire.LFwearL);
                        writer.WritePropertyName("LFwearM");
                        writer.WriteValue(lap.Pit.Tire.LFwearM);
                        writer.WritePropertyName("LFwearR");
                        writer.WriteValue(lap.Pit.Tire.LFwearR);

                        writer.WritePropertyName("RFwearL");
                        writer.WriteValue(lap.Pit.Tire.RFwearL);
                        writer.WritePropertyName("RFwearM");
                        writer.WriteValue(lap.Pit.Tire.RFwearM);
                        writer.WritePropertyName("RFwearR");
                        writer.WriteValue(lap.Pit.Tire.RFwearR);

                        writer.WritePropertyName("LRwearL");
                        writer.WriteValue(lap.Pit.Tire.LRwearL);
                        writer.WritePropertyName("LRwearM");
                        writer.WriteValue(lap.Pit.Tire.LRwearM);
                        writer.WritePropertyName("LRwearR");
                        writer.WriteValue(lap.Pit.Tire.LRwearR);

                        writer.WritePropertyName("RRwearL");
                        writer.WriteValue(lap.Pit.Tire.RRwearL);
                        writer.WritePropertyName("RRwearM");
                        writer.WriteValue(lap.Pit.Tire.RRwearM);
                        writer.WritePropertyName("RRwearR");
                        writer.WriteValue(lap.Pit.Tire.RRwearR);
                        //end of tire object
                        writer.WriteEndObject();
                    }

                    writer.WritePropertyName("Telemetry");
                    writer.WriteStartArray();
                    foreach (var value in ProgramConfig.GetTelemetryValuesIfInConfig(program.ProgramConfig.Telemetry, lap.Telemetry))
                    {
                        //telemetry object
                        writer.WriteStartObject();
                        foreach (var telemetry in value)
                        {
                            writer.WritePropertyName(telemetry.Key);
                            writer.WriteValue(telemetry.Value);
                        }
                        //end of telemetry object
                        writer.WriteEnd();
                    }
                    writer.WriteEndArray();
                    //end of lap object
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
            //data written

            writer.WriteEndObject();
            writer.Close();
            return sw.ToString();
        }
    }
}
