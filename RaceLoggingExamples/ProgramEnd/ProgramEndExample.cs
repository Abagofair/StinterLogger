using StinterLogger.RaceLogging.General.SimEventArgs;
using StinterLogger.RaceLogging;
using StinterLogger.RaceLogging.General.Program;
using StinterLogger.RaceLogging.Simulations.Iracing;
using System;
using System.IO;

namespace RaceLoggingExamples.ProgramEnd
{
    class ProgramEndExample
    {
        static void Main(string[] args)
        {
            ISimLogger iracing = new iRacingLogger(4.0f);
            IProgramLoader loader = new ProgramLoader();

            ProgramManager programManager = new ProgramManager(iracing, loader);
            programManager.ProgramEnd += OnProgramEnd;
            programManager.Load(Directory.GetCurrentDirectory(), "default.json");

            iracing.StartListening();
            //You have to be connected to iracing and in a session before the loaded program will start
            Console.WriteLine("waiting for an iracing connection..");
            while (!iracing.IsLive) { }
            Console.WriteLine("starting program..");
            programManager.StartProgram();
        }

        static void OnProgramEnd(object sender, ProgramEndEventArgs e)
        {
            Console.WriteLine("The program has finished..");
            Console.WriteLine("Program: " + e.ProgramData.ProgramConfig.Name);
            Console.WriteLine("Driver: " + e.ProgramData.Driver.DriverName);
            Console.WriteLine("Car: " + e.ProgramData.Driver.CarNameLong);
            Console.WriteLine("Track: " + e.ProgramData.Track.DisplayName);

            foreach (var laps in e.ProgramData.LapData)
            {
                Console.WriteLine("-----------------");
                Console.WriteLine("Fuel used: " + laps.CompletedLap.FuelUsed);
                Console.WriteLine("Incidents: " + laps.CompletedLap.Incidents);
                Console.WriteLine("LapTime: " + TimeSpan.FromSeconds(laps.CompletedLap.LapTime).ToString());
                Console.WriteLine("Lap number: " + laps.CompletedLap.LapNumber);
                int sectorCount = 1;
                foreach (var sector in laps.CompletedLap.SectorTimes)
                {
                    Console.WriteLine("\tSector " + sectorCount + ": " + TimeSpan.FromSeconds(sector).ToString());
                    ++sectorCount;
                }
                Console.WriteLine("-----------------");
            }
            Console.ReadLine();
        }
    }
}
