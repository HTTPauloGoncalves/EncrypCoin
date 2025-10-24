namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public record CoinMarketDto(
        string Id,
        string Symbol,
        string Name,
        decimal CurrentPrice,
        decimal MarketCap,
        decimal PriceChangePercentage24h
    );
}
