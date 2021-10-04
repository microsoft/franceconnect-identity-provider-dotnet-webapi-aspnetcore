// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
using Microsoft.AspNetCore.Http;
using System.IO;
using IdentityServer4.Services;
using WebApi_Identity_Provider_DotNet.Jwt;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IdentityServer4.Validation;
using IdentityServer4.Services.Default;
using Microsoft.Extensions.Hosting;

namespace WebApi_Identity_Provider_DotNet
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; set; }
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (string.IsNullOrEmpty(Configuration["FranceConnect:ClientSecret"]))
            {
                throw new InvalidOperationException("FC Client Secret not found. It must be added to the configuration, through User Secrets for example.");
                // User-Secrets documentation : https://docs.asp.net/en/latest/security/app-secrets.html
            }

            services.AddControllersWithViews();
            services.AddHealthChecks();
            // To add specific health checks, such as database probe, read more here https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1#create-health-checks-1

            var identityConfig = new IdentityInMemoryConfiguration(Configuration["FranceConnect:ClientId"], Configuration["FranceConnect:ClientSecret"], Configuration["FranceConnect:RedirectUri"]);
            services.AddSingleton(identityConfig);

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;

                options.UserInteraction.ErrorUrl = "/error";
                options.Discovery.ShowKeySet = false;
            });
            // The developer signing credential is intended for development, add your own on production environments
            builder.AddDeveloperSigningCredential();

            // remove default token validation & creation services, which do not support FranceConnect signature algorithm
            builder.Services.Remove(builder.Services.SingleOrDefault(s => s.ServiceType== typeof(ITokenCreationService)));
            builder.Services.Remove(builder.Services.SingleOrDefault(s => s.ServiceType == typeof(ITokenValidator)));

            // add our custom services working with the HS256 algorithm
            builder.Services.TryAddTransient<ITokenCreationService, FranceConnectTokenCreationService>();
            builder.Services.TryAddTransient<ITokenValidator, FranceConnectTokenValidator>();

            // in-memory resources. To use your own database, see https://identityserver4.readthedocs.io/en/release/quickstarts/8_entity_framework.html
            builder.AddTestUsers(identityConfig.TestUsers);
            builder.AddInMemoryIdentityResources(identityConfig.IdentityResources);
            builder.AddInMemoryClients(identityConfig.Clients);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
