using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modsmith.Models
{
    public class Mod
    {
        public string Name { get; set; }

        public string Filename { get; set; }
        public string SteamId { get; set; }
        public string Author { get; set; }
        public DateTime LastUpdated { get; set; }

        public string Type { get; set; } // todo - enum this

        public bool Checked { get; set; }
    }
}
