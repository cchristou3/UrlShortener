using API.Entities;
using API.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services
{
    public class UrlShorteningService
    {
        public const int NumberOfCharsInShortLink = 7;
        private const string AlphabetAndDigits = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly Random _random = new();
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;

        public UrlShorteningService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }

        public async Task<Result<string>> GetLongUrlAsync(string code)
        {
            var shortenedUrl = await _memoryCache.GetOrCreateAsync(code, async (entry) =>
            {
                // Should remain in the cache if no longer accessed for 90 days
                entry.SlidingExpiration = TimeSpan.FromDays(90);
                return await _context.ShortenedUrls
                    .Where(x => x.Code == code)
                    .Select(x => new { x.LongUrl })
                    .FirstOrDefaultAsync();
            });

            if (shortenedUrl is null)
            {
                return Result<string>.Failure(DomainErrors.NotFound);
            }

            return Result<string>.Success(shortenedUrl.LongUrl);
        }

        public async Task<Result<string>> ShortenUrlAsync(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return Result<string>.Failure(DomainErrors.InvalidUrl);
            }
            
            var code = await GenerateUniqueCode();

            var shortenedUrl = new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                LongUrl = url,
                Code = code,
                ShortUrl = $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/{code}"
            };

            await _context.ShortenedUrls.AddAsync(shortenedUrl);
            await _context.SaveChangesAsync();

            return Result<string>.Success(shortenedUrl.LongUrl);
        }

        private async Task<string> GenerateUniqueCode()
        {
            var codeChars = new char[NumberOfCharsInShortLink];

            while(true)
            {
                for (int i = 0; i < codeChars.Length; i++)
                {
                    codeChars[i] = AlphabetAndDigits[_random.Next(AlphabetAndDigits.Length - 1)];
                }

                string code = new string(codeChars);

                if (!await _context.ShortenedUrls.AnyAsync(x => x.Code == code))
                {
                    return code;
                }
            }
        }
    }
}
