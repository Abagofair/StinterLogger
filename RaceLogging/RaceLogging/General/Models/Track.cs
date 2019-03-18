using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.General.Models
{
    public class Track
    {
        public Track()
        {
            this.Sectors = new List<Sector>();
        }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Country { get; set; }

        public int Id { get; set; }

        public float Length { get; set; }

        public string WeatherType { get; set; }

        public float SurfaceTemp { get; set; }

        public float AirTemp { get; set; }

        public List<Sector> Sectors { get; set; }
    }
}
