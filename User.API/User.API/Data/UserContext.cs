using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Api.Models;

namespace User.Api.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AppUser>()
                .ToTable("User")
                .HasKey(m => m.Id);
            builder.Entity<UserProperty>().Property(u => u.Value).HasMaxLength(100);
            builder.Entity<UserProperty>().Property(u => u.Key).HasMaxLength(100);
            builder.Entity<UserProperty>().ToTable("UserProperties")
                .HasKey(u => new {u.Key, u.AppUserId, u.Value});

            builder.Entity<UserTag>().Property(u => u.Tag).HasMaxLength(100);
            builder.Entity<UserTag>().ToTable("UseTags").HasKey(u => new {u.UserId, u.Tag});
            builder.Entity<BPFile>().ToTable("BPFile").HasKey(u => u.Id);

            base.OnModelCreating(builder);
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserProperty> UserProperties { get; set; }
        public DbSet<UserTag> UserTags { get; set; }
    }
}
