using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApplication1.data;

namespace WebApplication1.Tests.Infrastructure;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Verwijder de SQL Server-configuratie.
            services.RemoveAll<
                IDbContextOptionsConfiguration<AppDbContext>>();

            services.RemoveAll<DbContextOptions<AppDbContext>>();

            // Houd de SQLite-database tijdens de test open.
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection =
                    new SqliteConnection("Data Source=:memory:");

                connection.Open();
                return connection;
            });

            // Gebruik SQLite in plaats van SQL Server.
            services.AddDbContext<AppDbContext>((provider, options) =>
            {
                var connection =
                    provider.GetRequiredService<DbConnection>();

                options.UseSqlite(connection);
            });
        });
    }
}
