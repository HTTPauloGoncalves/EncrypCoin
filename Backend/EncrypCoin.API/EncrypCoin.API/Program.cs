using EncrypCoin.API.Data;
using EncrypCoin.API.Middlewares;
using EncrypCoin.API.Repository.Implementations;
using EncrypCoin.API.Repository.Interfaces;
using EncrypCoin.API.Services.Application.Implementations;
using EncrypCoin.API.Services.Application.Interfaces;
using EncrypCoin.API.Services.External.Implementations;
using EncrypCoin.API.Services.External.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Context;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("serilog.json", optional: false, reloadOnChange: true);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// =========================
// 🔐 Configuração JWT
// =========================
var key = builder.Configuration["Jwt:Key"]
          ?? throw new InvalidOperationException("Jwt:Key não configurada");
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
// =========================
// 🧩 Política de Admin
// =========================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// =========================
// ⚙️ Controllers e Swagger
// =========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EncrypCoin API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT assim: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// =========================
// 🗺️ AutoMapper
// =========================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// =========================
// 🔴 Redis
// =========================
var redisConnection = builder.Configuration.GetConnectionString("Redis")
                     ?? throw new InvalidOperationException("Redis não configurado!");

var multiplexer = await ConnectionMultiplexer.ConnectAsync(redisConnection);

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// =========================
// 🗄️ Banco de Dados (PostgreSQL)
// =========================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não está configurado!");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// =========================
// 💼 Serviços da Aplicação
// =========================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// =========================
// 💰 CoinGecko HttpClient
// =========================
var baseUrl = builder.Configuration["CoinGecko:BaseUrl"]
              ?? throw new InvalidOperationException("CoinGecko:BaseUrl não está configurado!");

builder.Services.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "EncrypCoinApp/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// =========================
// 🚀 Build da Aplicação
// =========================
var app = builder.Build();

// =========================
// 🧪 Swagger (Apenas em Dev)
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// =========================
// ⚠️ Middleware Global de Erros
// =========================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            error = "Ocorreu um erro interno ao processar sua solicitação.",
            status = 500
        }));
    });
});
// =========================
// 🆔 Middleware de TraceId
// =========================
app.UseSerilogRequestLogging();
app.Use(async (ctx, next) =>
{
    var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;

    LogContext.PushProperty("TraceId", traceId);

    await next();
});

// =========================
// 🔒 Middlewares
// =========================
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();
app.UseExceptionMiddleware();


// =========================
// 📡 Map Controllers
// =========================
app.MapControllers();

app.MapGet("/test", () =>
{
    Log.Information("Testando log estruturado.");
    return "ok";
});

await app.RunAsync();
Console.WriteLine("Rodando em: http://localhost:5232/");