using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
{
    public class TimeEstimation
    {
        private List<float> _distances;

        private Dictionary<float, float> _distanceTime;

        public TimeEstimation()
        {
            this._distanceTime = new Dictionary<float, float>();
            this._distances = new List<float>();
        }

        public void ResetData()
        {
            this._distanceTime = new Dictionary<float, float>();
            this._distances = new List<float>();
        }

        public void AddDistanceTimeValue(float distance, float time)
        {
            //keep the data sorted - if we aint moving or going backwards, discard.
            //we have to reset the data after a completed tho
            //this._distanceTime.Add(distance, time);
            int count = this._distanceTime.Count;
            if (count > 0 && distance > this._distanceTime.Keys.ElementAt(count - 1))
            {
                this._distanceTime.Add(distance, time);
                this._distances.Add(distance);
            }
            else if (count == 0)
            {
                this._distanceTime.Add(distance, time);
            }
        }

        public float TravelTimeToPoint(float distance)
        {
            var seq = this._distanceTime.ToList();

            bool stop = false;
            var index = 0;
            for (int i = 0; i < seq.Count-1 && !stop; ++i)
            {
                var val = seq[i];
                if (val.Value < 1.0f && i > 0)
                {
                    index = i;
                    stop = true;
                }
            }

            seq.RemoveRange(0, index + 1);

            return this.GetYEstimate(seq, distance);
        }

        /// <summary>
        /// Least squared regression
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private float GetYEstimate(List<KeyValuePair<float,float>> xy, float x)
        {
            var distTimeAvg = this.DistTimeAvg(xy);

            float aSum = 0.0f;
            float bSum = 0.0f;
            float cSum = 0.0f;

            float b = distTimeAvg.Value;

            for (int i = 0; i < xy.Count - 1; ++i)
            {
                float x0 = xy[i].Key - distTimeAvg.Key;
                float y = xy[i].Value;

                aSum += x0 * x0;
                bSum += 2.0f * b * x0 - 2.0f * x0 * y;
                cSum += b * b - 2.0f * b * y + y * y;
            }

            float lineSlope = (-1.0f * (bSum / (2.0f * aSum)));

            return lineSlope * (x - distTimeAvg.Key) + distTimeAvg.Value;
        }

        public float GetSectorTime(float sectorDistanceFromStart)
        {
            float distanceToRetrieveTimeFor = 0.0f;
            float prevVal = 0.0f;
            foreach (var val in this._distances)
            {
                if (val < sectorDistanceFromStart)
                {
                    prevVal = val;
                    continue;
                }
                else if (val == sectorDistanceFromStart)
                {
                    distanceToRetrieveTimeFor = val;
                }
                else
                {
                    distanceToRetrieveTimeFor = prevVal;
                }
            }

            return this._distanceTime[distanceToRetrieveTimeFor];
        }

        private KeyValuePair<float, float> DistTimeAvg(List<KeyValuePair<float, float>> distTime)
        {
            float distanceAverage = 0.0f;
            float timeAverage = 0.0f;

            foreach (var val in distTime)
            {
                distanceAverage += val.Key;
                timeAverage += val.Value;
            }

            distanceAverage /= distTime.Count;
            timeAverage /= distTime.Count;

            return (new KeyValuePair<float, float>(distanceAverage, timeAverage));
        }
    }
}
