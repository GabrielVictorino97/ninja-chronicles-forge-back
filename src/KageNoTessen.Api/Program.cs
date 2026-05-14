using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using KageNoTessen.Application;
using KageNoTessen.Application.Battle;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using KageNoTessen.Infrastructure;
using KageNoTessen.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

var config = builder.Configuration;
var jwtSecret = config["Jwt:Secret"]!;

builder.Services.AddInfrastructure(config);
builder.Services.Configure<CombatBalanceOptions>(config.GetSection(CombatBalanceOptions.Section));
builder.Services.AddApplication();

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Kage no Tessen API";
        s.Version = "v1";
    };
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        };
        o.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(o => o.AddPolicy("web", p =>
{
    var origins = config["Cors:Origins"]?.Split(",") ?? ["http://localhost:5173"];
    p.WithOrigins(origins)
     .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    // Em desenvolvimento, aceita qualquer origem localhost (Vite/TanStack podem usar portas variadas).
    if (builder.Environment.IsDevelopment())
        p.SetIsOriginAllowed(origin => new Uri(origin).Host is "localhost" or "127.0.0.1");
}));

builder.Services.AddHealthChecks()
    .AddNpgSql(config.GetConnectionString("Default")!)
    .AddRedis(config["Redis:ConnectionString"]!);

var app = builder.Build();

app.UseSerilogRequestLogging();

// Global exception handler — converts known exceptions to proper HTTP status codes.
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (UnauthorizedAccessException ex)
    {
        ctx.Response.StatusCode = 401;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync($"{{\"detail\":\"{ex.Message}\"}}");
    }
    catch (InvalidOperationException ex)
    {
        ctx.Response.StatusCode = 400;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync($"{{\"detail\":\"{ex.Message}\"}}");
    }
    catch (ArgumentException ex)
    {
        ctx.Response.StatusCode = 400;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync($"{{\"detail\":\"{ex.Message}\"}}");
    }
});

app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Errors.ResponseBuilder = (failures, ctx, status) =>
    {
        var errors = failures.GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
        return Results.Json(new { errors }, statusCode: status);
    };
});

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

if (!app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

// Apply migrations and seed
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db, config["Admin:Email"]!, config["Admin:Password"]!);
}
catch (Exception ex) when (ex is InvalidOperationException or SocketException
                                     || ex.InnerException is SocketException
                                     || ex.GetBaseException() is SocketException)
{
    Log.Fatal("Banco de dados indisponível. Execute 'docker compose up -d' para iniciar PostgreSQL e Redis.");
    throw new InvalidOperationException(
        "Banco de dados indisponível (PostgreSQL não encontrado em localhost:5432). " +
        "Execute 'docker compose up -d' para subir as dependências.", ex);
}

app.UseSwaggerGen();
app.Run();
