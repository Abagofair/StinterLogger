﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.General.Program
{
    public class ProgramManagerException : Exception
    {
        public ProgramManagerException(string message) : base(message)
        {
        }
    }
}
