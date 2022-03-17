using Modsmith.Models;
using Modsmith.Services.Interfaces;
using Modsmith.Utils;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Modsmith.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IProfileManagerService _profileManagerService;

        public ConfigurationService(IProfileManagerService profileManagerService)
        {
            _profileManagerService = profileManagerService;
        }

        public void SetLastUsedProfileForGame(string profileName, uint appId)
        {
            var config = LoadConfiguration();
            var gameConfig = config.gameConfig.games.Find(game => game.appId == appId);
            gameConfig.lastUsedProfile = profileName;

            File.WriteAllText(DirectoryPathUtils.GetConfigurationFilePath(), JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public Profile? GetLastUsedProfileForGame(uint appId)
        {
            var config = LoadConfiguration();
            var gameConfig = config.gameConfig.games.Find(game => game.appId == appId);
            var lastUsedProfileName = gameConfig.lastUsedProfile;
            var profile = _profileManagerService.LoadProfile(appId, lastUsedProfileName);

            // todo - error handling
            if (profile is null)
            {
                throw new ArgumentException("No last used profile exists for this game");
            }

            return profile;
        }

        public ModsmithConfiguration LoadConfiguration()
        {
            var config = JsonConvert.DeserializeObject<ModsmithConfiguration>(File.ReadAllText(DirectoryPathUtils.GetConfigurationFilePath()));
            if (config is not null)
                return config;
            else
                throw new Exception("Configuration doesn't exist, try restarting the app to regenerate");
        }
    }
}
