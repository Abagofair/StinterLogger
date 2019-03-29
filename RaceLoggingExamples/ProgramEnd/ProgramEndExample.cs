using RaceLogging.General.SimEventArgs;
using RaceLogging;
using RaceLogging.General.Program;
using RaceLogging.Simulations.Iracing;
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
        }

        static void OnProgramEnd(object sender, ProgramEndEventArgs e)
        {
            Console.WriteLine("The program has finished..");
            Console.WriteLine("Program: " + e.SimProgram.ProgramConfig.Name);
            Console.WriteLine("Driver: " + e.SimProgram.Driver.DriverName);
            Console.WriteLine("Car: " + e.SimProgram.CompletedLaps[0].Car.Name);
            Console.WriteLine("Track: " + e.SimProgram.CompletedLaps[0].Track.Name);

            foreach (var laps in e.SimProgram.CompletedLaps)
            {
                Console.WriteLine("-----------------");
                Console.WriteLine("Fuel used: " + laps.FuelUsed);
                Console.WriteLine("Incidents: " + laps.Incidents);
                Console.WriteLine("LapTime: " + TimeSpan.FromSeconds(laps.Time.LapTime).ToString());
                Console.WriteLine("Lap number: " + laps.LapNumber);
                int sectorCount = 1;
                foreach (var sector in laps.Time.SectorTimes)
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
