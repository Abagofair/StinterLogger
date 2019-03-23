using System.Collections.Generic;

namespace RaceLogging.General.Entities
{
    public class Game
    {
        public Game()
        {
            this.Tracks = new List<Track>();
            this.Cars = new List<Car>();
        }

        public string Name { get; set; }

        public List<Track> Tracks { get; set; }

        public List<Car> Cars { get; set; }
    }
}
