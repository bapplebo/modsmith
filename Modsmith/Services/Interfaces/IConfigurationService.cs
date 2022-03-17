using Modsmith.Models;

namespace Modsmith.Services.Interfaces
{
    public interface IConfigurationService
    {
        void SetLastUsedProfileForGame(string profileName, uint appId);
        Profile GetLastUsedProfileForGame(uint appId);
        ModsmithConfiguration LoadConfiguration();
    }
}
