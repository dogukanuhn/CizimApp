using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppData.Helpers
{
    public class RedisHandler : IRedisHandler
    {
        private readonly ConnectionMultiplexer Connection;
        private readonly IDatabaseAsync db;
        //private readonly IConfiguration _configuration;
        //public RedisHandler(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //    Connection = ConnectionMultiplexer.Connect(_configuration.GetSection("Redis:Url").Value);
        //    db = Connection.GetDatabase(int.Parse(_configuration.GetSection("Redis:Database").Value));
        //}

        public RedisHandler()
        {
            Connection = ConnectionMultiplexer.Connect("localhost:6379");
            db = Connection.GetDatabase(1);
        }
        public async Task<string> GetFromCache(string key)
        {
            var isCached = await IsCached(key);
            if (!isCached)
            {
                
                return null;
            }
            var cachedData = await db.StringGetAsync(key);
            return await Task.FromResult(cachedData);

        }

        public async Task<bool> AddToCache(string key, TimeSpan timeout, string data)
        {
            var isCached = await IsCached(key);
            if (isCached)
            {
                return await Task.FromResult(false);
            }

            await db.StringSetAsync(key, data, timeout);
            return await Task.FromResult(true);
        }
     

        public async Task<bool> RemoveFromCache(string key)
        {
            
            var result = await db.KeyDeleteAsync(key);
            if (!result)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }
        public async Task<bool> IsCached(string key)
        {
            var cachedData = await db.KeyExistsAsync(key);
            if (!cachedData)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);

        }
    }
}

