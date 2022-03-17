using Modsmith.Models;
using Modsmith.Services.Interfaces;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Modsmith.Services
{
    public class SteamService : ISteamService
    {
        public void Test()
        {
            Debug.WriteLine("Init SteamService, so DI is working");
        }

        public bool IsSteamRunning()
        {
            Process[] processes = Process.GetProcessesByName("steam");
            return processes.Length > 0;
        }

        public async Task<List<Mod>> BulkFetchWorkshopInfo(List<uint> workshopIds)
        {
            PublishedFileId[] fileIds = workshopIds.Select(fileId => new PublishedFileId { Value = fileId }).ToArray();
            try
            {
                var query = Steamworks.Ugc.Query.Items.WithFileId(fileIds);
                var page = 1;
                var result = await query.GetPageAsync(page);
                var results = new List<Mod>();
                if (result.HasValue)
                {
                    List<Steamworks.Ugc.Item> entries = result.Value.Entries.ToList();
                    foreach (var entry in entries)
                    {
                        var mod = new Mod
                        {
                            Author = entry.Owner.Name,
                            Name = entry.Title,
                            SteamId = entry.Id.ToString(),
                            LastUpdated = entry.Updated
                        };

                        results.Add(mod);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching for workshop IDs {workshopIds}");
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public Mod FetchWorkshopInfo(uint workshopId)
        {
            throw new System.NotImplementedException();
        }
    }
}
