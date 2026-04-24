using SRp.Data;
using SRp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<RefreshOptions>(builder.Configuration.GetSection("Refresh"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(10) // маленький допуск по времени
        };

        options.IncludeErrorDetails = true;

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[JWT] FAILED: {ctx.Exception.GetType().Name}: {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                Console.WriteLine($"[JWT] CHALLENGE: error={ctx.Error} desc={ctx.ErrorDescription}");

                // Чтобы увидеть причину в игре
                if (!ctx.Response.HasStarted)
                {
                    ctx.Response.Headers["X-JWT-Error"] = $"{ctx.Error} | {ctx.ErrorDescription}";
                }

                return Task.CompletedTask;
            }
        };
    });

// Add services to the container.
builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = null;
    o.JsonSerializerOptions.DictionaryKeyPolicy = null;
});

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthorizationContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<SessionContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<RefreshTokenContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<CharactersContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<PlayerInventoryContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSingleton<TokenService>();

builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<FacePunchAuthService>();

builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();

app.Use(async (ctx, next) =>
{
    Console.WriteLine($"[MW] {ctx.Request.Method} {ctx.Request.Scheme}://{ctx.Request.Host}{ctx.Request.Path}");
    Console.WriteLine($"[MW] Authorization='{ctx.Request.Headers.Authorization}'");

    var endpoint = ctx.GetEndpoint();

    if (endpoint == null)
    {
        await next();
        return;
    }

    var isAllowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null;

    if (isAllowAnonymous)
    {
        await next();
        return;
    }

    var requiresAuth =
            endpoint.Metadata.GetMetadata<IAuthorizeData>() != null;

    if (!requiresAuth)
    {
        await next();
        return;
    }

    Console.WriteLine($"[MW] IsAuthenticated={ctx.User?.Identity?.IsAuthenticated}");

    if (!(ctx.User?.Identity?.IsAuthenticated ?? false))
    {
        Console.WriteLine($"[MW] sub={ctx?.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value}");
        Console.WriteLine($"[MW] sid={ctx?.User?.FindFirst("sid")?.Value}");

        Console.WriteLine("[MW] 401 reason: not authenticated"); // перед return в блоке IsAuthenticated=false
        ctx?.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    foreach (var c in ctx.User.Claims)
    {
        Console.WriteLine($"[MW] claim: {c.Type} = {c.Value}");
    }

    var sidStr = ctx.User.FindFirst("sid")?.Value;
    var subStr = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!Guid.TryParse(sidStr, out var sid) || !long.TryParse(subStr, out var steamId64))
    {
        Console.WriteLine("[MW] 401 reason: bad sid/sub"); // перед return где TryParse не прошёл
        ctx.Response.StatusCode = 401;
        return;
    }

    var sessionContext = ctx.RequestServices.GetRequiredService<SessionContext>();
    var sessionManager = new SessionService(sessionContext);

    var session = await sessionManager.FindActiveSessionById(sid);

    if (session == null || session.OwnerSteamId64 != steamId64)
    {
        Console.WriteLine("[MW] 401 reason: session not found or owner mismatch"); // перед return где session==null или Owner != sub
        ctx.Response.StatusCode = 401;
        return;
    }

    await sessionManager.TouchSession(session);

    await next();
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapHealthChecks("/healthz");

app.UseForwardedHeaders();
app.MapOpenApi();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
