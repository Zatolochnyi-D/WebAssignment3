using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStaticFiles();
app.UseDefaultFiles();
app.UseHttpsRedirection();

var redirectUri = "https://localhost:7101/callback";
var casdoorEndpoint = "https://localhost:10443";
var clientId = "017bef6a6dd3106efc51";
var clientSecret = "edfddea2f7258a679d5f44b4ed89032d3805b09e";

app.MapGet("/", context => Task.Run(() => context.Response.Redirect("/index.html")));

app.MapGet("/login", async context =>
{
    var redirectUrl = $"{casdoorEndpoint}/login/oauth/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=openid profile email";
    await Task.Run(() => context.Response.Redirect(redirectUrl));
});

app.MapGet("/callback", async (string code, HttpContext context) =>
{
    Console.WriteLine("Code: " + code);
    var tokenSource = "/api/login/oauth/access_token";

    using var client = new HttpClient();
    client.BaseAddress = new(casdoorEndpoint);

    var content = new FormUrlEncodedContent(
    [
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
        new KeyValuePair<string, string>("redirect_uri", redirectUri),
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
    Console.WriteLine("Token: " + encodedToken);
    var token = decoder.ReadJwtToken(encodedToken);
    return Results.Ok(new { id = token.Claims.First(x => x.Type == "id").Value, name = token.Claims.First(x => x.Type == "name").Value });
});

app.Run();