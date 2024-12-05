using UserApi.Models;
using UserApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration object to DI for using db
// Configuration settings fetch from app settings file
builder.Services.Configure<UsersDbSettings>(
    builder.Configuration.GetSection("UsersDatabase"));

// Service for use database
builder.Services.AddSingleton<UsersService>();

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
