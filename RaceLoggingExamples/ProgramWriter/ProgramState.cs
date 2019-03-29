using System;

namespace RaceLoggingExamples.SimpleConsoleApplication
{
    public class ProgramState : IState
    {
        public IState Execute(StateManager stateManager)
        {
            App.ModeMessage = "Enter L to load program and wait for start, S to stop active program, M to go back to main..";
            App.RenderAll();

            var input = Console.ReadKey();
            Console.ReadLine();
            if (input.Key == ConsoleKey.L)
            {
                App.ModeMessage = "Enter the program name..";
                App.RenderAll();
                App.ResetCursorToCommandPosition();
                var programName = Console.ReadLine();
                try
                {
                    App.ProgramManager.Load(App.AppPath, programName + ".json");
                    App.ProgramMessage = "The program has been loaded and will start when it can..";
                }
                catch (Exception e)
                {
                    App.ErrorMessage = e.Message;
                }
            }
            else if (input.Key == ConsoleKey.S)
            {
                if (App.ProgramManager.IsProgramActive)
                {
                    App.ProgramMessage = "The program has been stopped..";
                    App.ProgramManager.Stop();
                }
            }
            else if (input.Key == ConsoleKey.M)
            {
                return stateManager.GetState(States.Main, false);
            }

            return stateManager.GetState(States.Program, false);
        }
    }
}
