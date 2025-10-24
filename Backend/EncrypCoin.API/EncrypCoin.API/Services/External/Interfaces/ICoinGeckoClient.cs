using EncrypCoin.API.Dtos.External.Coingecko;

namespace EncrypCoin.API.Services.External.Interfaces
{
    public interface ICoinGeckoClient
    {
        Task<CoinPriceDto> GetPriceAsync(string coinId, string vsCurrency);
        Task<List<CoinListDto>> GetCoinsListAsync();
        Task<List<CoinMarketDto>> GetMarketsAsync(string vsCurrency, int perPage = 10, int page = 1);
        Task<TrendingResponseDto> GetTrendingAsync();
    }
}
