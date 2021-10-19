// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Services;

var builder = WebApplication.CreateBuilder(args);


// Database Services

var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFido2CredentialsStore, Fido2CredentialsStore>();

// Identity Services

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

builder.Services.AddTransient<IEmailSender, AuthMessageSender>();

// MVC & Pages
builder.Services.AddRazorPages(options =>
        {
            // Require a logged in user to access the logout page and all the Manage pages.
            // Using the [Authorize] attribute on Controllers, Razor Pages, or Action Methods, is another way to manage the access MVC/Razor Pages.
            options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
        })
    .AddNewtonsoftJson(); 
// the FIDO2 library requires NewtonsoftJson as of v2.0.2 . They are currently migrating it to System.Text.Json which will allow us to remove this call and the NewtonsoftJson dependency


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
app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
