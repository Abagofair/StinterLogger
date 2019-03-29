using System;
using System.Collections.Generic;
using System.Text;

namespace RaceLoggingExamples.SimpleConsoleApplication
{
    public class FuelState : IState
    {
        public IState Execute(StateManager stateManager)
        {
            App.ModeMessage = "Enter F to enable FuelManager, S to disable FuelManager, M to go back to main..";
            App.RenderAll();
            App.ResetCursorToCommandPosition();

            var input = Console.ReadKey();
            Console.ReadLine();
            if (input.Key == ConsoleKey.F)
            {
                App.FuelMessage = "The FuelManager has been enabled..";
                App.RenderAll();
                App.ResetCursorToCommandPosition();
                App.FuelManager.Enable();
            }
            else if (input.Key == ConsoleKey.S)
            {
                App.FuelMessage = "The FuelManager has been disabled..";
                App.RenderAll();
                App.ResetCursorToCommandPosition();
                App.FuelManager.Disable();
            }
            else if (input.Key == ConsoleKey.M)
            {
                return stateManager.GetState(States.Main, false);
            }

            return stateManager.GetState(States.Fuel, false);
        }
    }
}
