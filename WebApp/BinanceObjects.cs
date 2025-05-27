using System.Text.Json.Serialization;

namespace WebApp;

public record SymbolTicker
{
    [JsonPropertyName("e")] public required string EventType { get; init; }
    [JsonPropertyName("E")] public required long EventTime { get; init; }
    [JsonPropertyName("s")] public required string Symbol { get; init; }
    [JsonPropertyName("p")] public required string PriceChange { get; init; }
    [JsonPropertyName("P")] public required string PriceChangePercent { get; init; }
    [JsonPropertyName("w")] public required string WeightedAveragePrice { get; init; }
    [JsonPropertyName("x")] public required string FirstTradePrice { get; init; }
    [JsonPropertyName("c")] public required string LastPrice { get; init; }
    [JsonPropertyName("Q")] public required string LastQuantity { get; init; }
    [JsonPropertyName("b")] public required string BestBidPrice { get; init; }
    [JsonPropertyName("B")] public required string BestBidQuantity { get; init; }
    [JsonPropertyName("a")] public required string BestAskPrice { get; init; }
    [JsonPropertyName("A")] public required string BestAskQuantity { get; init; }
    [JsonPropertyName("o")] public required string OpenPrice { get; init; }
    [JsonPropertyName("h")] public required string HighPrice { get; init; }
    [JsonPropertyName("l")] public required string LowPrice { get; init; }
    [JsonPropertyName("v")] public required string TotalTradedBaseAssetVolume { get; init; }
    [JsonPropertyName("q")] public required string TotalTradedQuoteAssetVolume { get; init; }
    [JsonPropertyName("O")] public required long StatisticsOpenTime { get; init; }
    [JsonPropertyName("C")] public required long StatisticsCloseTime { get; init; }
    [JsonPropertyName("F")] public required long FirstTradeId { get; init; }
    [JsonPropertyName("L")] public required long LastTradeId { get; init; }
    [JsonPropertyName("n")] public required long TotalNumberOfTrades { get; init; }
}

public record BinanceResponse
{
    [JsonPropertyName("stream")] public required string Stream { get; init; }
    [JsonPropertyName("data")] public required SymbolTicker Data { get; init; }
}

public record SimplifiedTicker
{
    [JsonPropertyName("time")] public required long Time { get; init; }
    [JsonPropertyName("symbol")] public required string Symbol { get; init; }
    [JsonPropertyName("price")] public required string Price { get; init; }
    [JsonPropertyName("priceChangePercent")] public required string PriceChangePercent { get; init; }
}