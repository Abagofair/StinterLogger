using RaceLogging;
using RaceLogging.General.Program;
using RaceLogging.General.SimEventArgs;
using RaceLogging.Simulations.Iracing;
using System;
using System.IO;
using System.Reflection;

namespace RaceLoggingExamples
{
    public class ProgramWriterExample
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
            IProgramWriter writer = new ProgramWriter();
            Console.WriteLine("Writing program to disk..");

            var bytes = writer.WriteProgramToByteArray(e.SimProgram, System.IO.Compression.CompressionLevel.Optimal);

            File.WriteAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\test.prodata", bytes);

            var str = writer.WriteProgramToString(e.SimProgram);

            File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\test.json", str);

            Console.WriteLine("Finished writing program to disk..");
            Console.ReadLine();
        }
    }
}
