using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.FuelCalculator
{
    public class FuelCalculator
    {
        private float RemainingLaps(float remainingRaceTime, float averageLapTime, float additionalLaps)
        {
            if (remainingRaceTime < 0.0f || averageLapTime < 0.0f || additionalLaps < 0)
            {
                throw new ArgumentException();
            }

            return (remainingRaceTime / averageLapTime) + additionalLaps;
        }

        public float FuelNeededToFinish(float remainingRaceTime, float averageLapTime, float additionalLaps, float fuelUsedPerLap)
        {
            if (fuelUsedPerLap < 0.0f)
            {
                throw new ArgumentException();
            }

            return RemainingLaps(remainingRaceTime, averageLapTime, additionalLaps) * fuelUsedPerLap;
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
