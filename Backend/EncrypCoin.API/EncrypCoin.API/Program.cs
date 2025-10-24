using EncrypCoin.API.Data;
using EncrypCoin.API.Services.Aplication.Implementations;
using EncrypCoin.API.Services.Aplication.Interfaces;
using EncrypCoin.API.Services.External.Implementations;
using EncrypCoin.API.Services.External.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis")
                     ?? throw new InvalidOperationException("Redis não configurado!");

var multiplexer = await ConnectionMultiplexer.ConnectAsync(redisConnection);

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Banco de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não está configurado!");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// CoinGecko HttpClient
var baseUrl = builder.Configuration["CoinGecko:BaseUrl"]
              ?? throw new InvalidOperationException("CoinGecko:BaseUrl não está configurado!");

builder.Services.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "EncrypCoinApp/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Build
var app = builder.Build();

// Swagger dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// Middleware global de erros
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
