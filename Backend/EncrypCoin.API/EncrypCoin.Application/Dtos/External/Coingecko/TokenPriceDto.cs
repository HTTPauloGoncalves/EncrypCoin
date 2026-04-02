using System.Text.Json.Serialization;

namespace EncrypCoin.Application.Dtos.External.Coingecko
{
    public class TokenPriceDto
    {
        [JsonPropertyName("usd")]
        public decimal Usd { get; set; }
    }
}
