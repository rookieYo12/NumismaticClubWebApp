using AuthApi.Models;
using AuthApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add db configuration settings to DI
builder.Services.Configure<DbSettings>(
    builder.Configuration.GetSection("UsersDatabase"));

// Service for using db
builder.Services.AddSingleton<UserService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
