using Microsoft.EntityFrameworkCore;

namespace Labs.SystemDesign.UrlShortener.Data
{
    public class UrlShortenerContext : DbContext
    {
        public UrlShortenerContext(DbContextOptions<UrlShortenerContext> options) : base(options)
        {
        }

        public DbSet<Url> Urls { get; set; }
    }
}
