using System.Text.Json.Serialization;

namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public class CoinCategoryDto
    {
        [JsonPropertyName("category_id")]
        public string CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("market_cap")]
        public decimal? MarketCap { get; set; }

        [JsonPropertyName("market_cap_change_24h")]
        public decimal? MarketCapChange24h { get; set; }

        [JsonPropertyName("volume_24h")]
        public decimal? Volume24h { get; set; }
    }
}
