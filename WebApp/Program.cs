using System.Net.WebSockets;
using System.Text;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseDefaultFiles();
        app.UseHttpsRedirection();
        app.UseWebSockets();

        app.MapGet("/", (HttpContext context) => context.Response.Redirect("/index.html"));
        app.UseOAuthEndpoints();

        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var ws = await context.WebSockets.AcceptWebSocketAsync();
                var doIt = true;

                async void Spam()
                {
                    while (doIt)
                    {
                        if (ws.State == WebSocketState.Open)
                        {
                            var message = "Hello World!";
                            var bytes = Encoding.UTF8.GetBytes(message);
                            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                            await ws.SendAsync(arraySegment,
                                                WebSocketMessageType.Text,
                                                true,
                                                CancellationToken.None);
                            Console.WriteLine("Message sent.");
                        }
                        else if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
                        {
                            Console.WriteLine($"Web socket closed with status {ws.State}.");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Web socket closed with unexpected status {ws.State}.");
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                }

                async void Listen()
                {
                    Console.WriteLine("Listen is active.");
                    var buffer = new byte[1024 * 4];
                    while (ws.State == WebSocketState.Open)
                    {
                        Console.WriteLine("Listening to messages.");
                        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var text = Encoding.UTF8.GetString(buffer);
                            Console.WriteLine($"Message received: {text}");
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            doIt = false;
                            await ws.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.Empty, result.CloseStatusDescription, CancellationToken.None);
                        }
                    }
                }
                Parallel.Invoke(Spam, Listen);
                // var task1 = Spam();
                // var task2 = Listen();
                // await Task.WhenAll(task1, task2);

            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });

        app.Run();
    }
}