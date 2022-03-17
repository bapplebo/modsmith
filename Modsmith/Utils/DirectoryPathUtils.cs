using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modsmith.Utils
{
    public static class DirectoryPathUtils
    {
        public static string GetAppDataPathForGame(uint appId)
        {
            return appId switch
            {
                ((uint)GameUtils.Game.THREE_KINGDOMS) => "ThreeKingdoms",
                ((uint)GameUtils.Game.WARHAMMER_2) => "Warhammer2",
                _ => throw new ArgumentException($"AppId {appId} has no mapping"),
            };
        }

        public static string GetExecutableFromAppId(uint appId)
        {
            return appId switch
            {
                ((uint)GameUtils.Game.THREE_KINGDOMS) => "Three_Kingdoms.exe",
                ((uint)GameUtils.Game.WARHAMMER_2) => "Warhammer2.exe",
                _ => throw new ArgumentException($"AppId {appId} has no mapping"),
            };
        }

        public static string GetConfigurationRoot()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "modsmith");
        }

        public static string GetConfigurationFilePath()
        {
            return Path.Combine(GetConfigurationRoot(), "config.json");
        }

        public static string GetTotalWarAppDataPath(uint appId)
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "The Creative Assembly",
                GetAppDataPathForGame(appId)
            );

            return path;
        }

        public static string GetTotalWarScriptPath(uint appId)
        {
            return Path.Combine(GetTotalWarAppDataPath(appId), "scripts");
        }
    }
}
