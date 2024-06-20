using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace SecureFileStorageProvider.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Storage> Storages { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                string dbString = configuration.GetConnectionString("db");

                optionsBuilder.UseMySql(dbString, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Storage>(entity =>
            {
                entity.ToTable("Storage");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Email, "Email")
                    .IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.Username).HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
