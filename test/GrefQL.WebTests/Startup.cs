using GrefQL.Tests.Model.Northwind;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GrefQL.WebTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging();
            services.AddDbContext<NorthwindContext>(o =>
                {
                    o.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Northwind;Trusted_Connection=True;");
                    o.EnableGraphQL();
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Information);

            app.UseFileServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
