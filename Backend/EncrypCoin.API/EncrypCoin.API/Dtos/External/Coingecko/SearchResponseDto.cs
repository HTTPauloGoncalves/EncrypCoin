using System.Text.Json.Serialization;

namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public class SearchResponseDto
    {
        [JsonPropertyName("coins")]
        public List<SearchCoinDto> Coins { get; set; }
    }

    public class SearchCoinDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("api_symbol")]
        public string ApiSymbol { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("market_cap_rank")]
        public int? MarketCapRank { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonPropertyName("large")]
        public string Large { get; set; }
    }
}
