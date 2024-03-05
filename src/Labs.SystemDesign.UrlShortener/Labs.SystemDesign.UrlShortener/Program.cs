using Labs.SystemDesign.UrlShortener.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddDbContext<UrlShortenerContext>( options => options.UseNpgsql("ConnectionStrings:UrlDb"));
builder.Services.AddDbContext<UrlShortenerContext>( options => 
    options.UseNpgsql("User ID=postgres; Password=postgres; Host=192.168.68.125; Port=5432; Database=postgres;"));

builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = "192.168.68.125:6379");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
