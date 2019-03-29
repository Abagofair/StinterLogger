using System;
using System.Collections.Generic;
using System.Text;

namespace RaceLoggingExamples.SimpleConsoleApplication
{
    public interface IState
    {
        IState Execute(StateManager stateManager);
    }
}
