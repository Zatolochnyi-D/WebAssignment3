using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace WebApp;

public static class Assignment3Settings
{
    public static void UseAssignment3Settings(this WebApplication app)
    {
        app.UseStaticFiles();
        app.UseDefaultFiles();
        app.UseHttpsRedirection();
    }
}

public static class Assignment3Endpoints
{
    public static void MapAssignment3Endpoints(this WebApplication app)
    {
        app.MapGet("/", (HttpContext context) => context.Response.Redirect("/index.html"));

        app.MapGet("/login", (HttpContext context) =>
        {
            var redirectUrl = $"{Consts.CasdoorEndpoint}/login/oauth/authorize?client_id={Consts.ClientId}&response_type=code&redirect_uri={Consts.RedirectUri}&scope=openid profile email";
            context.Response.Redirect(redirectUrl);
        });

        app.MapGet("/callback", async (string code, HttpContext context) =>
        {
            var tokenSource = "/api/login/oauth/access_token";
            using var client = new HttpClient();
            client.BaseAddress = new(Consts.CasdoorEndpoint);
            var content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", Consts.ClientId),
                new KeyValuePair<string, string>("client_secret", Consts.ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", Consts.RedirectUri),
            ]);
            var response = await client.PostAsync(tokenSource, content);
            var contentStream = await response.Content.ReadAsStreamAsync();
            var text = Encoding.Default.GetString(await response.Content.ReadAsByteArrayAsync());
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(text);
            var token = dict!["access_token"].ToString()!;
            context.Response.Cookies.Append("access_token", token, new CookieOptions() { Expires = DateTime.Now.AddHours(2), Path = "/" });
            context.Response.Redirect("/");
        });

        app.MapGet("/getResource", (HttpContext context) =>
        {
            context.Request.Headers.TryGetValue("Authorization", out var result);
            var encodedToken = result.First();
            var decoder = new JwtSecurityTokenHandler();
            if (!decoder.CanReadToken(encodedToken))
            {
                return Results.Unauthorized();
            }
            var token = decoder.ReadJwtToken(encodedToken);
            return Results.Ok(new { id = token.Claims.First(x => x.Type == "id").Value, name = token.Claims.First(x => x.Type == "name").Value });
        });
    }
}