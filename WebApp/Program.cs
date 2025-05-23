using System.Net.WebSockets;
using System.Text;

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

            static async void SendMessages(WebSocket ws)
            {
                while (true)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        var message = "Hello World!";
                        var bytes = Encoding.UTF8.GetBytes(message);
                        var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                        Console.WriteLine("Message sent.");
                    }
                    else if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
                    {
                        Console.WriteLine($"Web socket closed with status {ws.State}.");
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
            static async void ReadMessages(WebSocket ws)
            {
                var buffer = new byte[1024 * 4];
                while (ws.State == WebSocketState.Open)
                {
                    Console.WriteLine("Listening to messages.");
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Message received: {text}");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.Empty, result.CloseStatusDescription, CancellationToken.None);
                    }
                }
            }
            Console.WriteLine("Connection opened.");
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            Parallel.Invoke(() => SendMessages(ws), () => ReadMessages(ws));
        });

        app.Run();
    }
}