using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApi_Identity_Provider_DotNet.Data;

namespace WebApiIdentityProviderDotNet.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("WebApi_Identity_Provider_DotNet.Models.Credential", b =>
                {
                    b.Property<string>("UserId")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PublicKeyHash");

                    b.Property<string>("ActiveChallenge");

                    b.Property<string>("DeviceName")
                        .IsRequired();

                    b.Property<string>("PublicKey")
                        .IsRequired();

                    b.HasKey("UserId", "PublicKeyHash");

                    b.ToTable("Credentials");
                });
        }
    }
}
