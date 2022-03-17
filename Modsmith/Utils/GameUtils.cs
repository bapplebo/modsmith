using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modsmith.Utils
{
    public static class GameUtils
    {
        public enum Game
        {
            WARHAMMER_2 = 594570,
            THREE_KINGDOMS = 779340
        }

        public static string AppIdToPrettyName(uint appId)
        {
            return appId switch
            {
                (uint)Game.WARHAMMER_2 => "Warhammer II",
                (uint)Game.THREE_KINGDOMS => "Three Kingdoms",
                _ => "Unknown game",
            };
        }
    }
}
