//
// The MIT License (MIT)
// Copyright (c) 2016 Microsoft France
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// You may obtain a copy of the License at https://opensource.org/licenses/MIT
//

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
            // Configuration loads behind the scenes since 2.0, with sources defined in program.cs https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/?view=aspnetcore-3.1#add-configuration-providers
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHealthChecks();
            //To add specific health checks, such as database probe https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1#create-health-checks-1

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
            });
            //The developer signing credential is intended for development, add your own on 
            builder.AddDeveloperSigningCredential();

            // remove default token validation & creation services, which do not support FranceConnect signature algorithm
            builder.Services.Remove(builder.Services.SingleOrDefault(s => s.ServiceType== typeof(ITokenCreationService)));
            builder.Services.Remove(builder.Services.SingleOrDefault(s => s.ServiceType == typeof(ITokenValidator)));

            // add our custom services working with the HS256 algorithm
            builder.Services.TryAddTransient<ITokenCreationService, FranceConnectTokenCreationService>();
            builder.Services.TryAddTransient<ITokenValidator, FranceConnectTokenValidator>();

            // in-memory, code 
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
