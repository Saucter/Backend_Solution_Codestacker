using PDF_Reader_APIs.Server;
using PDF_Reader_APIs.Shared.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace PDF_Reader_APIs.Server.Caching
{
    public class CacheRepository : ICacheRepository
    {
        protected readonly IMemoryCache Cache; //Inject the IMemoryCache DI
        protected readonly Database DB;
        public CacheRepository(Database DB, IMemoryCache Cache)
        {
            this.DB = DB;
            this.Cache = Cache;
        }

        public async Task<List<PDF>> GetCache(string CacheName, List<int>? id) //Take in the name of the cache and PDF IDs to look for
        {
            var CacheData = await Cache.GetOrCreateAsync(CacheName, async entry => //If a cache is available, return it. Otherwise create one
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); //Cache remains for 10 mins (Or if something posted/deleted)
                var Data = await DB.PDFs.Include(x => x.Sentences).ToListAsync();
                Cache.Set(CacheName, Data);
                return Data;
            });
            return (id.Count() == 0 ) ? CacheData : CacheData.Where(x => id.Contains(x.id)).ToList(); //Return either the entire cache or the specified ID
        }

        public async Task Remove(string CacheName)
        {
            Cache.Remove(CacheName); //Clear the cache
        }
    }
}
