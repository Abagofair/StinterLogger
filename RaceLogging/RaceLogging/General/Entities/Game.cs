using System.Collections.Generic;

namespace RaceLogging.General.Entities
{
    public class Game
    {
        public string Name { get; set; }

        public List<Track> Tracks { get; set; }

        public List<Car> Cars { get; set; }
    }
}
