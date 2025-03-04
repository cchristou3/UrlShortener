using API;
using API.Extensions;
using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});

builder.Services.AddScoped<UrlShorteningService>();

builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.MapPost("api/shorten", 
    async (
        ShortenUrlRequest request,
        UrlShorteningService urlShorteningService) =>
{
    var shortUrlResult = await urlShorteningService.ShortenUrlAsync(request.Url);
    return shortUrlResult.ToActionResult();
});

app.MapGet("api/{code}", async (
    string code, 
    UrlShorteningService urlShorteningService) =>
{
    var longUrlResult  = await urlShorteningService.GetLongUrlAsync(code);
    return longUrlResult.ToActionResult();
});

app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
