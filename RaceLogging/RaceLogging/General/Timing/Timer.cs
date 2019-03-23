using System;

namespace RaceLogging.Timing
{
    public class Timer
    {
        private bool _isActive;

        private long _startTimeTicks;

        private long _endTimeTicks;

        public void StartTimer()
        {
            if (!this._isActive)
            {
                this._startTimeTicks = DateTime.UtcNow.Ticks;
                this._isActive = true;
            }
        }

        public double StopTimer()
        {
            double deltaTime = -1.0f;
            if (this._isActive)
            {
                this._endTimeTicks = DateTime.UtcNow.Ticks;
                this._isActive = false;

                var delta = this._endTimeTicks - this._startTimeTicks;
                deltaTime = TimeSpan.FromTicks(delta).TotalSeconds;
            }
            return deltaTime;
        }
    }
}
