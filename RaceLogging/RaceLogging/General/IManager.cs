﻿using RaceLogging.General.Entities;
using System;

namespace RaceLogging.General
{
    public interface IManager<T> where T : EventArgs
    {
        event EventHandler<T> OnEntityChange;

        void Enable();
        void Disable();

        IEntity Entity { get; set; }
    }
}
