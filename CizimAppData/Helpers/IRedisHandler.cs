using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Helpers
{
    public interface IRedisHandler
    {
        Task<string> GetFromCache(string key);
        Task<bool> RemoveFromCache(string key);
        Task<bool> AddToCache(string key, TimeSpan timeout, string data);
        Task<bool> IsCached(string key);
    }
}
