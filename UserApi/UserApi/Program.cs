using UserApi.Models;
using UserApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration object to DI for using db
// Configuration settings fetch from app settings file
builder.Services.Configure<UsersDbSettings>(
    builder.Configuration.GetSection("UsersDatabase"));

// UsersService class is registered in DI as singleton
builder.Services.AddSingleton<UsersService>();

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
