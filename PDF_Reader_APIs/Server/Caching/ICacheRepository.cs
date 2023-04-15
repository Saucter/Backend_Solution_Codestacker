using PDF_Reader_APIs.Shared.Entities;

public interface ICacheRepository
{
    Task<List<PDF>> GetCache(string CacheName, List<int>? id);
    Task Remove(string CacheName);
}