using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.FuelCalculator
{
    public class FuelCalculator
    {
        private float RemainingLaps(float remainingRaceTime, float timeRaced, float lapsCompleted, float additionalLaps)
        {
            if (remainingRaceTime < 0.0f || timeRaced < 0.0f || additionalLaps < 0.0f || lapsCompleted < 0.0f)
            {
                throw new ArgumentException();
            }
            float averageLapTime = timeRaced / lapsCompleted;
            return (remainingRaceTime / averageLapTime) + additionalLaps;
        }

        public float FuelNeededToFinish(float remainingRaceTime, float timeRaced, float lapsCompleted, float additionalLaps, float fuelUsedPerLap)
        {
            if (fuelUsedPerLap < 0.0f)
            {
                throw new ArgumentException();
            }

            return RemainingLaps(remainingRaceTime, timeRaced, lapsCompleted, additionalLaps) * fuelUsedPerLap;
        }

        public float FuelNeededToFinish(float remainingLaps, float fuelUsedPerLap)
        {
            if (fuelUsedPerLap < 0.0f)
            {
                throw new ArgumentException();
            }

            return remainingLaps * fuelUsedPerLap;
        }
    }
}
