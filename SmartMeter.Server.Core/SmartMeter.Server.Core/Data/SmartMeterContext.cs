using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartMeter.Server.Core.Models;

namespace SmartMeter.Server.Core.Data
{
    public class SmartMeterContext : DbContext
    {
        public DbSet<Meter> Meters { get; set; }
        public DbSet<Reading> Readings { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<JWToken> JWTokens { get; set; }

        public SmartMeterContext(DbContextOptions<SmartMeterContext> options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meter>().HasKey(m => m.MeterId);
            modelBuilder.Entity<Reading>().HasKey(r => r.ReadingId);
            modelBuilder.Entity<Tariff>().HasKey(t => t.TariffId);
            modelBuilder.Entity<JWToken>().HasKey(j => j.JwtId);

            modelBuilder.Entity<Reading>()
                .HasOne(r => r.Meter)
                .WithMany()
                .HasForeignKey(r => r.MeterId);

            modelBuilder.Entity<Tariff>()
                .HasOne(t => t.Meter)
                .WithMany()
                .HasForeignKey(t => t.MeterId);

            modelBuilder.Entity<JWToken>()
                .HasOne(j => j.Reading)
                .WithMany()
                .HasForeignKey(j => j.ReadingId);
        }
    }
}
