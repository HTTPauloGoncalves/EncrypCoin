using System.Text.Json.Serialization;

namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public class MarketChartDto
    {
        [JsonPropertyName("prices")]
        public List<List<decimal>> Prices { get; set; }

        [JsonPropertyName("market_caps")]
        public List<List<decimal>> MarketCaps { get; set; }

        [JsonPropertyName("total_volumes")]
        public List<List<decimal>> TotalVolumes { get; set; }
    }
}
