using EncrypCoin.API.Services.Aplication.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace EncrypCoin.API.Services.Aplication.Implementations
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer multiplexer)
            => _database = multiplexer.GetDatabase();

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _database.StringGetAsync(key);
            if (data.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(data!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiration);
        }
    }
}

