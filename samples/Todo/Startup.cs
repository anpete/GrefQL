using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Models;

namespace Todo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging();
            services.AddDbContext<TodoContext>(o =>
                {
                    o.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Todos;Trusted_Connection=True;");
                    o.EnableGraphQL();
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Information);

            app.UseFileServer();
            app.UseMvcWithDefaultRoute();
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseDefaultConfiguration(args)
                .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                .UseUrls("http://localhost:5000")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
