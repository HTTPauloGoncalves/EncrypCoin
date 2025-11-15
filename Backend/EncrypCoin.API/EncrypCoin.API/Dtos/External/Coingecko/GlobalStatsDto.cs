using System.Text.Json.Serialization;

namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public class GlobalStatsDto
    {
        [JsonPropertyName("data")]
        public GlobalStatsData Data { get; set; }
    }

    public class GlobalStatsData
    {
        [JsonPropertyName("active_cryptocurrencies")]
        public int ActiveCryptocurrencies { get; set; }

        [JsonPropertyName("upcoming_icos")]
        public int UpcomingIcos { get; set; }

        [JsonPropertyName("ongoing_icos")]
        public int OngoingIcos { get; set; }

        [JsonPropertyName("ended_icos")]
        public int EndedIcos { get; set; }

        [JsonPropertyName("markets")]
        public int Markets { get; set; }

        [JsonPropertyName("total_market_cap")]
        public Dictionary<string, decimal> TotalMarketCap { get; set; }

        [JsonPropertyName("total_volume")]
        public Dictionary<string, decimal> TotalVolume { get; set; }

        [JsonPropertyName("market_cap_percentage")]
        public Dictionary<string, decimal> MarketCapPercentage { get; set; }

        [JsonPropertyName("market_cap_change_percentage_24h_usd")]
        public decimal MarketCapChange24hUsd { get; set; }
    }
}
