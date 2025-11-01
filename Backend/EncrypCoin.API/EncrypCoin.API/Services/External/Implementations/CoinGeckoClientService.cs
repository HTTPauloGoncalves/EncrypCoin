using EncrypCoin.API.Dtos;
using EncrypCoin.API.Dtos.External.Coingecko;
using EncrypCoin.API.Services.Application.Interfaces;
using EncrypCoin.API.Services.Base;
using EncrypCoin.API.Services.External.Interfaces;
using Microsoft.Extensions.Logging;

namespace EncrypCoin.API.Services.External.Implementations
{
    public class CoinGeckoClient : BaseHttpService, ICoinGeckoClient
    {
        private readonly ICacheService _cache;
        private readonly ILogger _logger;

        public CoinGeckoClient(HttpClient client, ILogger<CoinGeckoClient> logger, ICacheService cache) : base(client, logger)
        { 
            _cache = cache;
            _logger = logger;
        }

        public async Task<CoinPriceDto> GetPriceAsync(string coinId, string vsCurrency)
        {

            string key = $"price:{coinId}:{vsCurrency}";
            var cached = await _cache.GetAsync<CoinPriceDto>(key);

            if (cached != null)
            {
                _logger.LogInformation("Pegando preço {CoinId}/{VsCurrency} do cache", coinId, vsCurrency);
                return cached;
            }

            var response = await GetJsonAsync<Dictionary<string, Dictionary<string, decimal>>>(
                $"simple/price?ids={coinId}&vs_currencies={vsCurrency}");

            if (response.TryGetValue(coinId, out var priceData) && priceData.TryGetValue(vsCurrency, out var price))
            {
                var result = new CoinPriceDto
                {
                    CoinId = coinId,
                    VsCurrency = vsCurrency,
                    Price = price
                };

                await _cache.SetAsync(key, result, TimeSpan.FromMinutes(2));
                _logger.LogInformation("Pegando preço {CoinId}/{VsCurrency} da API e salvando no cache", coinId, vsCurrency);
                return result;
            }

            throw new HttpRequestException($"Preço não encontrado para {coinId}/{vsCurrency}");
        }


        public async Task<List<CoinListDto>> GetCoinsListAsync()
        {
            string key = $"coins:list";
            var cached = await _cache.GetAsync<List<CoinListDto>>(key);

            if (cached != null)
            {
                _logger.LogInformation("Pegando lista de coins do cache");
                return cached;
            }
            
            var result = await GetJsonAsync<List<CoinListDto>>("coins/list");
            await _cache.SetAsync(key, result, TimeSpan.FromHours(1));

            Console.WriteLine("Pegando lista de coins da API e salvando no cache com 1 hora");

            return result;
        }

        public async Task<List<CoinMarketDto>> GetMarketsAsync(string vsCurrency, int perPage = 10, int page = 1)
        {
            string key = $"markets:{vsCurrency}:{perPage}:{page}";
            var cached = await _cache.GetAsync<List<CoinMarketDto>>(key);
            if (cached != null)
            {
                _logger.LogInformation("Pegando markets do cache");
                return cached;
            }

            var result = await GetJsonAsync<List<CoinMarketDto>>($"coins/markets?vs_currency={vsCurrency}&order=market_cap_desc&per_page={perPage}&page={page}&sparkline=false");
            await _cache.SetAsync(key, result, TimeSpan.FromMinutes(10));

            _logger.LogInformation("Pegando markets da API e salvando no cache com 5 minutos");

            return result;
        }

        public async Task<TrendingResponseDto> GetTrendingAsync()
        { 
            string key = $"trending";
            var cached =  await _cache.GetAsync<TrendingResponseDto>(key);

            if (cached != null)
            {
                _logger.LogInformation("Pegando trending do cache");
                return cached;
            }

            var result = await GetJsonAsync<TrendingResponseDto>("search/trending");
            await _cache.SetAsync(key, result, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Pegando trending da API e salvando no cache com 30 minutos");

            return result;
        }
    }
}
