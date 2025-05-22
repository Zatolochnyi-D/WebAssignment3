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

        app.MapGet("/", (HttpContext context) => context.Response.Redirect("/index.html"));
        app.UseOAuthEndpoints();

        app.Run();
    }
}