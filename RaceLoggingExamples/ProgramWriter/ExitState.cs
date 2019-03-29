using System;
using System.Collections.Generic;
using System.Text;

namespace RaceLoggingExamples.SimpleConsoleApplication
{
    public class ExitState : IState
    {
        public IState Execute(StateManager stateManager)
        {
            App.Kill();
            return this;
        }
    }
}
