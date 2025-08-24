using Microsoft.EntityFrameworkCore;
using ImageApi.Models;

namespace ImageApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ImageFile> Images => Set<ImageFile>();
    public DbSet<Person> Persons => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(127);
            b.Property(x => x.PartName).IsRequired().HasMaxLength(127);
            b.Property(x => x.PhotoFileName).IsRequired().HasMaxLength(255);
        });
    }
}
