
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebEmpresa.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("ApiEmpresa", client =>
{
    client.BaseAddress = new Uri("http://localhost:5125/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AresTech.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Path = "/";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = false;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.Redirect("/login");
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] =
        "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "form-action 'self'; " +
        "frame-ancestors 'self'; " +
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "style-src 'self' 'unsafe-inline' https://gc.kis.v2.scr.kaspersky-labs.com; " +
        "script-src 'self'; " +
        "connect-src 'self' ws: wss:;";

    await next();
});

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/session/login", async (
    HttpContext context,
    IHttpClientFactory factory) =>
{
    if (!context.Request.IsHttps &&
        !app.Environment.IsDevelopment())
    {
        return Results.BadRequest(
            new { mensaje = "Se requiere HTTPS." });
    }

    var origin = context.Request.Headers.Origin.ToString();

    if (!string.IsNullOrWhiteSpace(origin) &&
        !string.Equals(
            origin,
            $"{context.Request.Scheme}://{context.Request.Host}",
            StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest(
            new { mensaje = "Origen no permitido." });
    }

    var solicitud =
        await context.Request.ReadFromJsonAsync<LoginWebDto>();

    if (solicitud == null ||
        string.IsNullOrWhiteSpace(solicitud.Nombre_usuario) ||
        string.IsNullOrWhiteSpace(solicitud.Password))
    {
        return Results.BadRequest(
            new { mensaje = "Ingrese usuario y contraseña." });
    }

    var api = factory.CreateClient("ApiEmpresa");

    using var respuesta = await api.PostAsJsonAsync(
        "api/Auth/login",
        new
        {
            Nombre_usuario = solicitud.Nombre_usuario.Trim(),
            Password = solicitud.Password
        });

    if (!respuesta.IsSuccessStatusCode)
    {
        return Results.Json(
            new { mensaje = "Usuario o contraseña incorrectos." },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    var datos =
        await respuesta.Content.ReadFromJsonAsync<LoginApiDto>();

    if (datos == null || string.IsNullOrWhiteSpace(datos.Token))
    {
        return Results.Json(
            new { mensaje = "La API no devolvió un token válido." },
            statusCode: StatusCodes.Status502BadGateway);
    }

    JwtSecurityToken jwt;

    try
    {
        jwt = new JwtSecurityTokenHandler().ReadJwtToken(datos.Token);
    }
    catch
    {
        return Results.Json(
            new { mensaje = "El token recibido no es válido." },
            statusCode: StatusCodes.Status502BadGateway);
    }

    var issuer = builder.Configuration["Jwt:Issuer"] ?? "ApiEmpresa";
    var audience = builder.Configuration["Jwt:Audience"] ?? "WebEmpresa";

    if (jwt.ValidTo <= DateTime.UtcNow ||
        jwt.Issuer != issuer ||
        !jwt.Audiences.Contains(audience))
    {
        return Results.Json(
            new
            {
                mensaje = "El token recibido no cumple la configuración esperada."
            },
            statusCode: StatusCodes.Status502BadGateway);
    }

    var idUsuario = jwt.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

    var nombreUsuario = jwt.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

    if (string.IsNullOrWhiteSpace(idUsuario) ||
        string.IsNullOrWhiteSpace(nombreUsuario))
    {
        return Results.Json(
            new { mensaje = "El token no contiene la identidad del usuario." },
            statusCode: StatusCodes.Status502BadGateway);
    }

    var identidad = new ClaimsIdentity(
        new[]
        {
            new Claim(ClaimTypes.NameIdentifier, idUsuario),
            new Claim(ClaimTypes.Name, nombreUsuario),
            new Claim("api_token", datos.Token)
        },
        CookieAuthenticationDefaults.AuthenticationScheme);

    var principal = new ClaimsPrincipal(identidad);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        });

    return Results.Ok(new { mensaje = "Sesión iniciada." });
});

app.MapPost("/session/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

    return Results.Ok(new { mensaje = "Sesión cerrada." });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal sealed class LoginWebDto
{
    public string Nombre_usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

internal sealed class LoginApiDto
{
    public string Token { get; set; } = string.Empty;
}
