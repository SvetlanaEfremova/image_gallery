using Microsoft.EntityFrameworkCore;
using Image_gallery.Models;
namespace Image_gallery
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Image> Images { get; set; } = null!;
        public ApplicationContext (DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Image>().HasData(
                new Image {Id = 1, FileName = "cat.jpeg", Category = "animals"});
        }
        public async Task ClearImages()
        {
            var allImages = await Images.ToListAsync();
            Images.RemoveRange(allImages);
            await SaveChangesAsync();
        }
    }
}
