namespace EncrypCoin.API.Dtos.External.Coingecko
{
    public record TrendingItemDto(
    string Id,
    string Name,
    string Symbol,
    int Market_cap_rank,
    double Price_btc,
    int Score
    );
}
