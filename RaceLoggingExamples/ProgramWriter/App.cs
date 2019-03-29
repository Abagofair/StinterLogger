using RaceLogging;
using RaceLogging.General.Fuel;
using RaceLogging.General.Program;
using RaceLogging.General.SimEventArgs;
using RaceLogging.Simulations.Iracing;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RaceLoggingExamples.SimpleConsoleApplication
{
    public class App
    {
        public static bool _exit = false;

        public static int[] ModeMessagePosition { get; } = { 0, 0 };
        public static string ModeMessage { get; set; }

        public static int[] CommandMessagePosition { get; } = { 0, 1 };
        public static string CommandMessage { get; set; }

        public static int[] ProgramMessagePosition { get; } = { 0, 3 };
        public static string ProgramMessage { get; set; }

        public static int[] FuelMessagePosition { get; } = { 0, 4 };
        public static string FuelMessage { get; set; }

        public static int[] ErrorMessagePosition { get; } = { 0, 5 };
        public static string ErrorMessage { get; set; }

        private static ISimLogger _simLogger = new iRacingLogger(4.0f);
        public static ISimLogger SimLogger { get => _simLogger; }

        private static FuelManager _fuelManager = new FuelManager(_simLogger, new RaceLogging.General.Debug.DebugManager(), 1);
        public static FuelManager FuelManager { get => _fuelManager; }

        private static IProgramLoader _programLoader = new ProgramLoader();

        private static ProgramManager _programManager = new ProgramManager(_simLogger, _programLoader);
        public static ProgramManager ProgramManager { get => _programManager; }

        public static string AppPath { get => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }

        static void Main(string[] args)
        {
            ModeMessage = "Press P for ProgramManager, F for the FuelManager, Q to exit..";
            CommandMessage = "Command: ";
            ProgramMessage = "Program manager not loaded";
            FuelMessage = "Fuel manager not enabled";
            ErrorMessage = "No errors";

            _programManager.ProgramEnd += OnProgramEnd;
            _programManager.ProgramActivated += OnProgramActivation;

            var stateManager = new StateManager();

            IState currentState = stateManager.GetState(States.Main, true);

            _simLogger.StartListening();
            //You have to be connected to iracing and in a session before the loaded program will start
            while (!_exit)
            {
                Console.Clear();
                currentState = currentState.Execute(stateManager);
                Thread.Sleep(250);
            }
        }

        public static void Kill()
        {
            _simLogger.StopListening();
            _exit = true;
        }

        public static void ResetCursorToCommandPosition()
        {
            Console.SetCursorPosition(CommandMessage.Length, CommandMessagePosition[1]);
        }

        public static void RenderAll()
        {
            Render(ProgramMessagePosition, ProgramMessage);
            Render(ModeMessagePosition, ModeMessage);
            Render(FuelMessagePosition, FuelMessage);
            Render(ErrorMessagePosition, ErrorMessage);
            Render(CommandMessagePosition, CommandMessage);
            ResetCursorToCommandPosition();
        }

        static void Render(int[] pos, string message)
        {
            Console.SetCursorPosition(pos[0], pos[1]);
            for (int i = 0; i < Console.WindowWidth; ++i)
            {
                Console.Write(' ');
            }
            Console.SetCursorPosition(pos[0], pos[1]);
            Console.Write(message);
        }

        static string Output()
        {
            Console.SetCursorPosition("program name: ".Length, 0);
            return Console.ReadLine();
        }

        static void OnProgramEnd(object sender, ProgramEndEventArgs e)
        {
            Console.SetCursorPosition(0, 6);
            IProgramWriter writer = new ProgramWriter();
            Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\output");

            string fileName = "\\output\\" + "Output-" + Guid.NewGuid();

            var bytes = writer.WriteProgramToByteArray(e.SimProgram, System.IO.Compression.CompressionLevel.Optimal);

            File.WriteAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\"+fileName+".prodata", bytes);

            var str = writer.WriteProgramToString(e.SimProgram);

            File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\"+fileName+".json", str);

            ProgramMessage = "Program has finished and been written to disk in \\output\\";
            RenderAll();
            ResetCursorToCommandPosition();
        }

        static void OnProgramActivation(object sender, EventArgs eventArgs)
        {
            ProgramMessage = "Program is now active..";
            RenderAll();
            ResetCursorToCommandPosition();
        }
    }
}
