using Biogenom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Infostructure.Data
{
    public class BiogenomDbContext: DbContext
    {
        public BiogenomDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Item> Items { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<AnalysisRequest> AnalysisRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemMaterial>()
                .HasKey(im => new { im.ItemId, im.MaterialId });

            modelBuilder.Entity<ItemMaterial>()
                .HasOne(im => im.Item)
                .WithMany(i => i.ItemMaterials)
                .HasForeignKey(im => im.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemMaterial>()
                .HasOne(im => im.Material)
                .WithMany(m => m.ItemMatelials)
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
