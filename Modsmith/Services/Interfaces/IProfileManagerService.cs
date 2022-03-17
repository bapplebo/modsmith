using Modsmith.Models;
using System.Collections.Generic;

namespace Modsmith.Services.Interfaces
{
    public interface IProfileManagerService
    {
        List<string> GetProfileNamesForGame(uint appId);
        void SaveProfile(uint appId, string name, List<Mod> mods);
        Profile LoadProfile(uint appId, string name);
        void DeleteProfile(uint appId, string name);
        bool ValidateProfileIsValid(string path);
    }
}
