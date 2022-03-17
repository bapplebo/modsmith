using Modsmith.Models;
using Modsmith.Services.Interfaces;
using Modsmith.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Modsmith.Services
{
    public class ProfileManagerService : IProfileManagerService
    {
        public void DeleteProfile(uint appId, string name)
        {
            throw new NotImplementedException();
        }

        public List<string> GetProfileNamesForGame(uint appId)
        {
            var profilesForGamePath = Path.Combine(DirectoryPathUtils.GetConfigurationRoot(), "profiles", DirectoryPathUtils.GetAppDataPathForGame(appId));
            if (!Directory.Exists(profilesForGamePath))
            {
                Directory.CreateDirectory(profilesForGamePath);
            }
            var profileNames = Directory.EnumerateFiles(profilesForGamePath, "*.*", SearchOption.TopDirectoryOnly)
                .Select(profilePath => Path.GetFileNameWithoutExtension(profilePath));

            return profileNames.ToList();
        }

        public Profile LoadProfile(uint appId, string profileName)
        {
            // todo - error handling
            var profilePath = Path.Combine(
                DirectoryPathUtils.GetConfigurationRoot(),
                "profiles",
                DirectoryPathUtils.GetAppDataPathForGame(appId),
                $"{profileName}.json"
            );

            var profile = JsonConvert.DeserializeObject<Profile>(
                File.ReadAllText(profilePath)
            );

            return profile;
        }

        public void SaveProfile(uint appId, string profileName, List<Mod> mods)
        {
            var gameConfigurationPath = Path.Combine(DirectoryPathUtils.GetConfigurationRoot(), "profiles", DirectoryPathUtils.GetAppDataPathForGame(appId));
            var profile = new Profile
            {
                Name = profileName,
                Mods = mods
            };

            var profilePath = Path.Combine(gameConfigurationPath, $"{profileName}.json");

            if (!Directory.Exists(gameConfigurationPath))
            {
                Directory.CreateDirectory(gameConfigurationPath);
            }

            var jsonFile = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(profilePath, jsonFile);
        }

        public bool ValidateProfileIsValid(string path)
        {
            throw new NotImplementedException();
        }
    }
}
