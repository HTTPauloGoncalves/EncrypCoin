using EncrypCoin.API.Dtos.External.Coingecko;

namespace EncrypCoin.API.Services.External.Interfaces
{
    public interface ICoinGeckoClient
    {
        Task<CoinPriceDto> GetPriceAsync(string coinId, string vsCurrency);
        Task<List<CoinListDto>> GetCoinsListAsync();
        Task<List<CoinMarketDto>> GetMarketsAsync(string vsCurrency, int perPage = 10, int page = 1);
        Task<TrendingResponseDto> GetTrendingAsync();
        Task<object> CheckAPIAsync();
        Task<List<object>> GetVsCurrenciesListAsync();
        Task<GlobalStatsDto> GetGlobalStatsAsync();
        Task<List<CoinCategoryDto>> GetCategoriesAsync();
        Task<SearchResponseDto> SearchAsync(string query);
        Task<MarketChartDto> GetMarketChartAsync(string coinId, string vsCurrency, int days);
        Task<Dictionary<string, TokenPriceDto>> GetTokenPriceAsync(
        string platformId, string contractAddress, string vsCurrency = "usd");
    }
}
