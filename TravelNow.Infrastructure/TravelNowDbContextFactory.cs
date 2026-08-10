using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TravelNow.Infrastructure;

public sealed class TravelNowDbContextFactory : IDesignTimeDbContextFactory<TravelNowDbContext>
{
    public TravelNowDbContext CreateDbContext(string[] args)
    {
        var configurationDirectory = FindConfigurationDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(configurationDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? BuildConnectionStringFromPassword(configuration["TRAVELNOW_DB_PASSWORD"]);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No database connection string was found. Set ConnectionStrings__DefaultConnection " +
                "or TRAVELNOW_DB_CONNECTION before running EF migrations.");
        }

        var options = new DbContextOptionsBuilder<TravelNowDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(TravelNowDbContext).Assembly.FullName))
            .Options;

        return new TravelNowDbContext(options);
    }

    private static string FindConfigurationDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, "TravelNow")
        };

        return candidates.FirstOrDefault(directory =>
            File.Exists(Path.Combine(directory, "appsettings.json")))
            ?? currentDirectory;
    }

    private static string? BuildConnectionStringFromPassword(string? password)
    {
        return string.IsNullOrWhiteSpace(password)
            ? null
            : $"Host=localhost;Port=5433;Database=TravelNowDb;Username=postgres;Password={password}";
    }
}
