using System.Collections.Immutable;

namespace WebApp;

public static class Consts
{
    public const string RedirectUri = "https://localhost:7101/callback";
    public const string CasdoorEndpoint = "https://localhost:10443";
    public const string ClientId = "017bef6a6dd3106efc51";
    public const string ClientSecret = "edfddea2f7258a679d5f44b4ed89032d3805b09e";

    public const string BinanceWebsocketsEndpoint = "wss://stream.binance.com:9443";
    public const string BinanceStreamParamName = "/stream?streams=";
    public static readonly ImmutableArray<string> Symbols = ["btc", "eth"];
    public static readonly string CombinedSymbols = string.Join("/", Symbols.Select(x => $"{x}usdt@ticker"));
}