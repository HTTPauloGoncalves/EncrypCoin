using System.Text.Json.Serialization;

namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public class TokenPriceDto
    {
        [JsonPropertyName("usd")]
        public decimal Usd { get; set; }
    }
}
