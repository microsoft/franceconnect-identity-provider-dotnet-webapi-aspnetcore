// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp_IdentityProvider_MFA.Models;

namespace WebApp_IdentityProvider_MFA.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Fido2Credential> Fido2Credentials { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Fido2Credential>()
            .HasKey(m => m.Id);

        base.OnModelCreating(builder);

    }

}

