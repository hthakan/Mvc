// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MvcSandbox
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Latest);
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseEndpointRouting(routes =>
            {
                routes.MapApplication();

                routes.MapGet(
                    requestDelegate: WriteEndpoints,
                    pattern: "/endpoints",
                    displayName: "Home");

                routes.MapControllerRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRazorPages();

                routes.MapHealthChecks("/healthz");
            });

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.UseEndpoint();
        }

        private static Task WriteEndpoints(HttpContext httpContext)
        {
            var dataSource = httpContext.RequestServices.GetRequiredService<CompositeEndpointDataSource>();

            var sb = new StringBuilder();
            sb.AppendLine("Endpoints:");
            foreach (var endpoint in dataSource.Endpoints.OfType<RouteEndpoint>().OrderBy(e => e.RoutePattern.RawText, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"- {endpoint.RoutePattern.RawText} '{endpoint.DisplayName}'");
            }

            var response = httpContext.Response;
            response.StatusCode = 200;
            response.ContentType = "text/plain";
            return response.WriteAsync(sb.ToString());
        }

        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args)
                .Build();

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(factory =>
                {
                    factory
                        .AddConsole()
                        .AddDebug();
                })
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>();
    }
}

