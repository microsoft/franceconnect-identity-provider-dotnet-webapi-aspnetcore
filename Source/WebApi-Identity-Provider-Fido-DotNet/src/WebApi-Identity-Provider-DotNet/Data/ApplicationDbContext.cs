using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi_Identity_Provider_DotNet.Models;

namespace WebApi_Identity_Provider_DotNet.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base ()
        { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        
        public DbSet<Credential> Credentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Credential>(b =>
            {
                b.HasKey(c => new { c.UserId, c.PublicKeyHash });

                b.Property(c => c.UserId).HasMaxLength(256);
                b.Property(c => c.UserId).IsRequired();
                b.Property(c => c.PublicKey).IsRequired();
                b.Property(c => c.PublicKeyHash).IsRequired();
                b.Property(c => c.DeviceName).IsRequired();
            });
        }
    }
}
