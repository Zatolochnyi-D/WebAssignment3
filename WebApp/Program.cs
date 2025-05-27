using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();

        app.UseAssignment3Settings();
        app.UseWebSockets();

        app.MapAssignment3Endpoints();
        app.Map("/ws", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            Console.WriteLine("Connection opened.");

            using var clientWs = await context.WebSockets.AcceptWebSocketAsync();
            using var binanceWs = new ClientWebSocket();
            var target = Consts.BinanceWebsocketsEndpoint + Consts.BinanceStreamParamName + Consts.CombinedSymbols;
            await binanceWs.ConnectAsync(new Uri(target), CancellationToken.None);

            var listenClient = ListenToClient(clientWs);
            var listenRemote = ListenToRemote(binanceWs, SendMessagesHandler(clientWs));
            await Task.WhenAny(listenClient, listenRemote);

            if (clientWs.State == WebSocketState.Open)
                await CloseWebSocketAsync(clientWs);
            if (binanceWs.State == WebSocketState.Open)
                await CloseWebSocketAsync(binanceWs);

            await Task.WhenAll(listenClient, listenRemote);

            Console.WriteLine("Connection closed.");
        });

        app.Run();
    }

    private static async Task ListenToClient(WebSocket clientWs)
    {
        while (clientWs.State == WebSocketState.Open)
        {
            var result = await clientWs.ReceiveAsync(ArraySegment<byte>.Empty, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
                await CloseWebSocketAsync(clientWs);
        }
    }

    private static async Task ListenToRemote(WebSocket binanceWs, Func<string, Task> messageHandler)
    {
        var buffer = new byte[1024 * 4];
        while (binanceWs.State == WebSocketState.Open)
        {
            var result = await binanceWs.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
                await messageHandler.Invoke(Encoding.UTF8.GetString(buffer, 0, result.Count));
            else if (result.MessageType == WebSocketMessageType.Close)
                if (binanceWs.State == WebSocketState.Open)
                    await CloseWebSocketAsync(binanceWs);
        }
    }

    private static Func<string, Task> SendMessagesHandler(WebSocket clientWs)
    {
        return async message =>
        {
            if (clientWs.State != WebSocketState.Open)
                return;

            var data = JsonSerializer.Deserialize<BinanceResponse>(message)!.Data;
            var simplifiedData = new SimplifiedTicker()
            {
                Time = data.EventTime,
                Symbol = data.Symbol,
                Price = data.LastPrice,
                PriceChangePercent = data.PriceChangePercent
            };
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(simplifiedData));
            await clientWs.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        };
    }

    private static async Task CloseWebSocketAsync(WebSocket ws)
    {
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
}