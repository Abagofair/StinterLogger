using System;
using System.Collections.Generic;
using System.Text;

namespace RaceLoggingExamples.SimpleConsoleApplication
{
    public enum States
    {
        Fuel, Main, Program, Exit
    }

    public class StateManager
    {
        private FuelState _fuelState;
        private MainState _mainState;
        private ProgramState _programState;
        private ExitState _exitState;

        public StateManager()
        {
            this._mainState = new MainState();
        }

        public IState GetState(States states, bool getNew)
        {
            IState state = null;
            if (states == States.Main)
            {
                if (getNew || this._mainState == null)
                {
                    this._mainState = new MainState();
                }
                state = this._mainState;
            }
            else if (states == States.Fuel)
            {
                if (getNew || this._fuelState == null)
                {
                    this._fuelState = new FuelState();
                }
                state = this._fuelState;
            }
            else if (states == States.Program)
            {
                if (getNew || this._programState == null)
                {
                    this._programState = new ProgramState();
                }
                state = this._programState;
            }
            else if (states == States.Exit)
            {
                if (getNew || this._exitState == null)
                {
                    this._exitState = new ExitState();
                }
                state = this._exitState;
            }
            return state;
        }
    }
}
