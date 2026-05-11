using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using NarutoPlayers.Application;
using Microsoft.EntityFrameworkCore;
using NarutoPlayers.Infrastructure;
using NarutoPlayers.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var config = builder.Configuration;
var jwtSecret = config["Jwt:Secret"]!;

builder.Services.AddInfrastructure(config);
builder.Services.AddApplication();

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Naruto Players API";
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
    p.WithOrigins(config["Cors:Origins"]?.Split(",") ?? ["http://localhost:5173"])
     .AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

builder.Services.AddHealthChecks()
    .AddNpgSql(config.GetConnectionString("Default")!)
    .AddRedis(config["Redis:ConnectionString"]!);

var app = builder.Build();

app.UseSerilogRequestLogging();
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db, config["Admin:Email"]!, config["Admin:Password"]!);
}

app.UseSwaggerGen();
app.Run();
