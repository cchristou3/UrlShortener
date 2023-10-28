using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class UrlShorteningService
    {
        public const int NumberOfCharsInShortLink = 7;
        private const string AlphabetAndDigits = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly Random _random = new();
        private readonly ApplicationDbContext _context;

        public UrlShorteningService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateUniqueCode()
        {
            var codeChars = new char[NumberOfCharsInShortLink];

            while(true)
            {
                for (int i = 0; i < codeChars.Length; i++)
                {
                    int randomIndex = _random.Next(AlphabetAndDigits.Length - 1);
                    codeChars[i] = AlphabetAndDigits[randomIndex];
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
