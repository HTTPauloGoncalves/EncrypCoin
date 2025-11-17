using EncrypCoin.API.Dtos.External.Coingecko;
using EncrypCoin.API.Services.External.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncrypCoin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoinsController : ControllerBase
    {
        private readonly ICoinGeckoClient _coinGeckoClient;

        public CoinsController(ICoinGeckoClient coinGeckoClient)
            => _coinGeckoClient = coinGeckoClient;

        // ------------------------------
        // Preço da moeda
        // ------------------------------
        [Authorize]
        [HttpGet("prices/{coinId}/{vsCurrency}")]
        [ProducesResponseType(typeof(CoinPriceDto), 200)]
        public async Task<IActionResult> GetPrice(string coinId, string vsCurrency)
        {
            if (string.IsNullOrWhiteSpace(coinId) || string.IsNullOrWhiteSpace(vsCurrency))
                return BadRequest("coinId e vsCurrency são obrigatórios.");

            var dto = await _coinGeckoClient.GetPriceAsync(coinId, vsCurrency);
            return Ok(dto);
        }

        // ------------------------------
        // Lista de moedas
        // ------------------------------
        [Authorize]
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<CoinListDto>), 200)]
        public async Task<IActionResult> GetCoinsList()
        {
            var dtos = await _coinGeckoClient.GetCoinsListAsync();
            return Ok(dtos);
        }

        // ------------------------------
        // Markets (moedas ordenadas por market cap)
        // ------------------------------
        [Authorize]
        [HttpGet("markets/{vsCurrency}/{perPage:int}/{page:int}")]
        [ProducesResponseType(typeof(List<CoinMarketDto>), 200)]
        public async Task<IActionResult> GetMarkets(string vsCurrency, int perPage, int page)
        {
            if (string.IsNullOrWhiteSpace(vsCurrency))
                return BadRequest("vsCurrency é obrigatório.");

            var dtos = await _coinGeckoClient.GetMarketsAsync(vsCurrency, perPage, page);
            return Ok(dtos);
        }

        // ------------------------------
        // Trending
        // ------------------------------
        [Authorize]
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending()
        {
            var trending = await _coinGeckoClient.GetTrendingAsync();
            return Ok(trending.Coins.Select(c => c.Item));
        }

        // ------------------------------
        // Ping (verifica status da API)
        // ------------------------------
        [Authorize]
        [HttpGet("check-api")]
        public async Task<IActionResult> CheckAPIAsync()
        {
            var status = await _coinGeckoClient.CheckAPIAsync();
            return Ok(status);
        }

        // ------------------------------
        // Lista de vs-currencies
        // ------------------------------
        [Authorize]
        [HttpGet("vs-currencies/list")]
        public async Task<IActionResult> GetVsCurrenciesListAsync()
        {
            var currencies = await _coinGeckoClient.GetVsCurrenciesListAsync();
            return Ok(currencies);
        }

        // ------------------------------
        // /global (estatísticas globais)
        // ------------------------------
        [Authorize]
        [HttpGet("global")]
        [ProducesResponseType(typeof(GlobalStatsDto), 200)]
        public async Task<IActionResult> GetGlobal()
        {
            var global = await _coinGeckoClient.GetGlobalStatsAsync();
            return Ok(global);
        }

        // ------------------------------
        // /coins/categories
        // ------------------------------
        [Authorize]
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CoinCategoryDto>), 200)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _coinGeckoClient.GetCategoriesAsync();
            return Ok(categories);
        }

        // ------------------------------
        // /search?query=bitcoin
        // ------------------------------
        [Authorize]
        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchResponseDto), 200)]
        public async Task<IActionResult> SearchAsync([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("query é obrigatório.");

            var result = await _coinGeckoClient.SearchAsync(query);
            return Ok(result);
        }

        // ------------------------------
        // /coins/{id}/market_chart
        // ------------------------------
        [Authorize]
        [HttpGet("chart/{coinId}/{vsCurrency}/{days:int}")]
        [ProducesResponseType(typeof(MarketChartDto), 200)]
        public async Task<IActionResult> GetMarketChart(string coinId, string vsCurrency, int days)
        {
            if (days <= 0)
                return BadRequest("days deve ser maior que zero.");

            var chart = await _coinGeckoClient.GetMarketChartAsync(coinId, vsCurrency, days);
            return Ok(chart);
        }

        // ------------------------------
        // Token price (ERC-20 / BEP-20 / Solana / Polygon)
        // ------------------------------
        [Authorize]
        [HttpGet("token-price/{platformId}/{contractAddress}/{vsCurrency}")]
        public async Task<IActionResult> GetTokenPrice(
            string platformId, string contractAddress, string vsCurrency)
        {
            if (string.IsNullOrWhiteSpace(platformId) ||
                string.IsNullOrWhiteSpace(contractAddress) ||
                string.IsNullOrWhiteSpace(vsCurrency))
            {
                return BadRequest("platformId, contractAddress e vsCurrency são obrigatórios.");
            }

            var price = await _coinGeckoClient.GetTokenPriceAsync(platformId, contractAddress, vsCurrency);
            return Ok(price);
        }
    }
}
