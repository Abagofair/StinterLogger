using System.Collections.Generic;

namespace RaceLogging.General.Entities
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

        public List<Sector> Sectors { get; set; }
    }
}
