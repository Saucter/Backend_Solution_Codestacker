using PDF_Reader_APIs.Server;
using PDF_Reader_APIs.Shared.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace PDF_Reader_APIs.Server.Caching
{
    public class CacheRepository : ICacheRepository
    {
        protected readonly IMemoryCache Cache;
        protected readonly Database DB;
        public CacheRepository(Database DB, IMemoryCache Cache)
        {
            this.DB = DB;
            this.Cache = Cache;
        }

        public async Task<List<PDF>> GetCache(string CacheName, List<int>? id)
        {
            var CacheData = await Cache.GetOrCreateAsync(CacheName, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var Data = await DB.PDFs.Include(x => x.Sentences).ToListAsync();
                Cache.Set(CacheName, Data);
                return Data;
            });
            return (id.Count() == 0 ) ? CacheData : CacheData.Where(x => id.Contains(x.id)).ToList();
        }

        public async Task Remove(string CacheName)
        {
            Cache.Remove(CacheName);
        }
    }
}
