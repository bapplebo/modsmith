using Modsmith.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modsmith.Services.Interfaces
{
    public interface ISteamService
    {
        void Test();
        bool IsSteamRunning();

        Task<List<Mod>> BulkFetchWorkshopInfo(List<uint> workshopIds);

        Mod FetchWorkshopInfo(uint workshopId);
    }
}
