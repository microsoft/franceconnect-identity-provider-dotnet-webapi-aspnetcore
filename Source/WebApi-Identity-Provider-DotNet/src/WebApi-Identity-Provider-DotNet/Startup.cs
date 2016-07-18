using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi_Identity_Provider_DotNet.Configuration;
using WebApi_Identity_Provider_DotNet.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using IdentityServer4.Core.Services;
using WebApi_Identity_Provider_DotNet.Jwt;
using IdentityServer4.Core.Services.Default;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IdentityServer4.Core.Validation;

namespace WebApi_Identity_Provider_DotNet
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            _environment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var builder = services.AddIdentityServer();
            // remove default services
            builder.Services.Remove(builder.Services.SingleOrDefault(s => s.ImplementationType == typeof(DefaultTokenSigningService)));
            builder.Services.Remove(builder.Services.SingleOrDefault(s => s.ImplementationType == typeof(TokenValidator)));
            // add custom services
            builder.Services.TryAddTransient<ITokenSigningService, FranceConnectTokenSigningService>();
            builder.Services.TryAddTransient<ITokenValidator, FranceConnectTokenValidator>();
            builder.AddInMemoryClients(Clients.Get());
            builder.AddInMemoryScopes(Scopes.Get());
            builder.AddInMemoryUsers(Users.Get());

            // Add framework services.
            services.AddMvc();
            services.AddTransient<SignInService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
