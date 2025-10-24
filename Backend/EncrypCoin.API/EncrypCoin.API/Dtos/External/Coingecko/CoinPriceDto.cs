namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public record CoinPriceDto
    {
        public string? CoinId { get; init; }
        public string? VsCurrency { get; init; }
        public decimal Price { get; init; }
    }
}
