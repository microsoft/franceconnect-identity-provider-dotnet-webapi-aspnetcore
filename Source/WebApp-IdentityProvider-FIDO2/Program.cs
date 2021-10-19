using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddTransient<IEmailSender, AuthMessageSender>();

builder.Services.AddRazorPages(options =>
        {
            //Require a logged in user to access the logout page and the all the Manage pages.
            options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
        });
// Using the [Authorize] attribute on Controllers, Razor Pages, or Action Methods, is another way to manage the access MVC/Razor Pages.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
