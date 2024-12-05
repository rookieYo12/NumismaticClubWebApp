using NumismaticClub.Models;
using NumismaticClub.Services;
using UserApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration object to DI for using db
// Configuration settings fetch from app settings file
builder.Services.Configure<CoinsDatabaseSettings>(
    builder.Configuration.GetSection("CoinsDatabase"));

// CoinsService class is registered in DI as singleton
builder.Services.AddSingleton<CoinsService>();

// Add Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Set connection string for Redis, fetched from app settings file
    options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];
    
    // Instance for name in Redis cache
    options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
});

// Add consumer to DI like background service
builder.Services.AddHostedService<ConsumerService>();

// Processing service of Kafka messages
builder.Services.AddSingleton<RequestProcessingService>();

builder.Services.AddSingleton<ProducerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
