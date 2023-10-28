using API;
using API.Entities;
using API.Extensions;
using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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
        UrlShorteningService urlShorteningService,
        ApplicationDbContext dbContext,
        HttpContext httpContext) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var _))
    {
        return Results.BadRequest("The specified Url is invalid.");
    }

    var code = await urlShorteningService.GenerateUniqueCode();

    var shortenedUrl = new ShortenedUrl
    {
        Id = Guid.NewGuid(),
        LongUrl = request.Url,
        Code = code,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}"
    };

    await dbContext.ShortenedUrls.AddAsync(shortenedUrl);
    await dbContext.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);
});

app.MapGet("api/{code}", async (
    string code, 
    ApplicationDbContext dbContext,
    IMemoryCache memoryCache) =>
{
    var shortenedUrl = await memoryCache.GetOrCreateAsync(code, async (entry) =>
    {
        // Should remain in the cache if no longer accessed for 90 days
        entry.SlidingExpiration = TimeSpan.FromDays(90);
        return await dbContext.ShortenedUrls
        .Where(x => x.Code == code)
        .Select(x => new { x.LongUrl })
        .FirstOrDefaultAsync();
    });

    if (shortenedUrl == null)
    {
        Results.NotFound();
    }

    return Results.Redirect(shortenedUrl!.LongUrl);
});

app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
