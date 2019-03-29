using System;

namespace RaceLoggingExamples.SimpleConsoleApplication
{
    public class MainState : IState
    {
        public IState Execute(StateManager stateManager)
        {
            App.ModeMessage = "Press P for ProgramManager, F for the FuelManager, Q to exit..";
            App.CommandMessage = "Command: ";
            App.ErrorMessage = "No errors";

            App.RenderAll();
            IState newState = this;
            //listen for input
            var input = Console.ReadKey();
            Console.ReadLine();
            if (input.Key == ConsoleKey.P)
            {
                newState = stateManager.GetState(States.Program, false);
            }
            else if (input.Key == ConsoleKey.F)
            {
                newState = stateManager.GetState(States.Fuel, false);
            }
            else if (input.Key == ConsoleKey.Q)
            {
                newState = stateManager.GetState(States.Exit, false);
            }
            return newState;
        }
    }
}
