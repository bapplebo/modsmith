using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modsmith.Models
{
    public class ModsmithConfiguration
    {
        public GameConfig gameConfig { get; set; }
    }

    public class GameConfig
    {
        public List<Game> games { get; set; }
    }

    public class Game
    {
        public uint appId { get; set; }
        public string directoryPath { get; set; }
        public string exe { get; set; }
        public string lastUsedProfile { get; set; }
    }
}
