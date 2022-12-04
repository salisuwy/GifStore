using GifStore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GifStore.Data
{
    public class GifStoreDbContext : DbContext
    {
        public GifStoreDbContext(DbContextOptions options) : base(options){ }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItemTag>()
                .HasKey(it => new { it.ItemId, it.TagId });
            modelBuilder.Entity<ItemTag>()
                .HasOne(it => it.Item)
                .WithMany(i => i.ItemTag)
                .HasForeignKey(it => it.ItemId);
            modelBuilder.Entity<ItemTag>()
                .HasOne(it => it.Tag)
                .WithMany(i => i.ItemTag)
                .HasForeignKey(it => it.TagId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ItemTag> ItemTags { get; set; }
    }
}
