using AspIdentityClient.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspIdentityClient.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasMaxLength(100).IsRequired();
            //modelBuilder.Entity<HistoryRow>().HasKey(x => x.MigrationId);

            //modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
            //modelBuilder.Entity<IdentityUserRole<string>>().HasKey(x => new { x.UserId, x.RoleId });
            //modelBuilder.Entity<IdentityUserToken<string>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

            //modelBuilder.Entity<ApplicationUser>(entity => entity.Property(m => m.Id).HasMaxLength(127));
            //modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.Id).HasMaxLength(127));
            //modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            //{
            //    entity.Property(m => m.LoginProvider).HasMaxLength(127);
            //    entity.Property(m => m.ProviderKey).HasMaxLength(127);
            //});
            //modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            //{
            //    entity.Property(m => m.UserId).HasMaxLength(127);
            //    entity.Property(m => m.RoleId).HasMaxLength(127);
            //});
            //modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            //{
            //    entity.Property(m => m.UserId).HasMaxLength(127);
            //    entity.Property(m => m.LoginProvider).HasMaxLength(127);
            //    entity.Property(m => m.Name).HasMaxLength(127);
            //});
        }
    }
}