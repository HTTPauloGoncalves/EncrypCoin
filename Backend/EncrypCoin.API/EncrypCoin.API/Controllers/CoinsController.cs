using EncrypCoin.API.Dtos.External.Coingecko;
using EncrypCoin.API.Services.External.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EncrypCoin.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class CoinsController : ControllerBase
    {
        private readonly ICoinGeckoClient _coinGeckoClient;

        public CoinsController(ICoinGeckoClient coinGeckoClient)
                => _coinGeckoClient = coinGeckoClient;

        [HttpGet("prices/{coinId}/{vsCurrency}")]
        [ProducesResponseType(typeof(CoinPriceDto), 200)]
        public async Task<IActionResult> GetPrice(string coinId, string vsCurrency)
        {
            if (string.IsNullOrWhiteSpace(coinId) || string.IsNullOrWhiteSpace(vsCurrency))
                return BadRequest("coinId e vsCurrency são obrigatórios.");

            var dto = await _coinGeckoClient.GetPriceAsync(coinId, vsCurrency);
            return Ok(dto);
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(List<CoinListDto>), 200)]
        public async Task<IActionResult> GetCoinsList()
        {
            var dtos = await _coinGeckoClient.GetCoinsListAsync();
            return Ok(dtos);
        }

        [HttpGet("markets/{vsCurrency}")]
        [ProducesResponseType(typeof(List<CoinMarketDto>), 200)]
        public async Task<IActionResult> GetMarkets(string vsCurrency)
        {
            if (string.IsNullOrWhiteSpace(vsCurrency))
                return BadRequest("vsCurrency é obrigatório.");

            var dtos = await _coinGeckoClient.GetMarketsAsync(vsCurrency);
            return Ok(dtos);
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending()
        {
            var trending = await _coinGeckoClient.GetTrendingAsync();
            return Ok(trending.Coins.Select(c => c.Item));
        }
    }
}



