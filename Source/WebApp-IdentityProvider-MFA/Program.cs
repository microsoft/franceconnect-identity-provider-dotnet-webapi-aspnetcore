// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApp_IdentityProvider_MFA;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Services;

var builder = WebApplication.CreateBuilder(args);

#region Database Services

var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFido2CredentialsStore, Fido2CredentialsStore>();
#endregion
#region Identity Services

builder.Services.AddFido2(options =>
{
    options.ServerDomain = builder.Configuration["FIDO2:ServerDomain"];
    options.ServerName = builder.Configuration["FIDO2:ServerName"];
    options.Origin = builder.Configuration["FIDO2:Origin"];
    options.TimestampDriftTolerance = builder.Configuration.GetValue<int>("FIDO2:TimestampDriftTolerance");
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<FIDO2TwoFactorProvider>(FIDO2TwoFactorProvider.Constants.ProviderName);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
#endregion
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();

#region IdentityServer & FranceConnect

if (string.IsNullOrEmpty(builder.Configuration["FranceConnect:ClientSecret"]))
{
    throw new InvalidOperationException("FC Client Secret not found. It must be added to the configuration, through User Secrets for example.");
    // User-Secrets documentation : https://docs.asp.net/en/latest/security/app-secrets.html
}

IdentityInMemoryConfiguration identityConfig = new(builder.Configuration["FranceConnect:ClientId"], builder.Configuration["FranceConnect:ClientSecret"], builder.Configuration["FranceConnect:RedirectUri"]);
builder.Services.AddSingleton(identityConfig);

var identityServerBuilder = builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
    options.EmitStaticAudienceClaim = true;

    options.UserInteraction.ErrorUrl = "/error";

    //We use symmetric keys with FranceConnect so this endpoint is not relevant
    options.Discovery.ShowKeySet = false;
});
identityServerBuilder.AddInMemoryIdentityResources(identityConfig.IdentityResources);
identityServerBuilder.AddInMemoryClients(identityConfig.Clients);
identityServerBuilder.AddAspNetIdentity<ApplicationUser>();

// Instead of adding a valid asymmetric credential through builder.AddSigningCredential,
// we use internal methods to manually add our signing and validation key credential (HS256, the only signing mechanism supported by FranceConnect as of today, which is symmetric and thus refused by builder.AddSigningCredential).
// This is a workaround as FranceConnect currently does not support assymetric signing keys.
SigningCredentials franceConnectSigningCredential = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identityConfig.FranceConnectSecret)), "HS256");
SecurityKeyInfo franceConnectSecuritykeyInfo = new()
{
    Key = franceConnectSigningCredential.Key,
    SigningAlgorithm = franceConnectSigningCredential.Algorithm
};

identityServerBuilder.Services.AddSingleton<ISigningCredentialStore>(new InMemorySigningCredentialsStore(franceConnectSigningCredential));
identityServerBuilder.Services.AddSingleton<IValidationKeysStore>(new InMemoryValidationKeysStore(new[] { franceConnectSecuritykeyInfo }));
#endregion


builder.Services.AddRazorPages(options =>
        {
            // Require a logged in user to access the logout page and all the Manage pages.
            // Using the [Authorize] attribute on Controllers, Razor Pages, or Action Methods, is another way to manage the access MVC/Razor Pages.
            options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
        })
    .AddNewtonsoftJson(); 
// the FIDO2 library requires NewtonsoftJson as of v2.0.2 . They are currently migrating it to System.Text.Json which will allow us to remove this call and the NewtonsoftJson dependency


builder.Services.AddHealthChecks();
// To add specific health checks, such as database probe, read more here https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1#create-health-checks-1


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// UseIdentityServer includes a call to useAuthentication so it is not necessary.
app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();

app.MapHealthChecks("/health");

app.Run();
