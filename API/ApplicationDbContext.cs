using API.Entities;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortenedUrl>(builder =>
            {
                builder.Property(s => s.Code)
                .HasMaxLength(UrlShorteningService.NumberOfCharsInShortLink)
                .IsUnicode(false)
                .IsFixedLength();

                builder.HasIndex(x => x.Code).IsUnique();
            });
        }
    }
}
