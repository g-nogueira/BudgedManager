using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonthlyBudget.Infrastructure.Database;
using Testcontainers.PostgreSql;

namespace MonthlyBudget.Integration.Tests;

/// <summary>
/// Shared fixture: spins up a real PostgreSQL container once per test collection.
/// Each test calls CreateClient() to get its own HttpClient with fresh headers.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    // Keep a single client for backwards compat — tests should use CreateClient() instead
    public HttpClient Client => _factory.CreateClient();

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace DB context with test container connection
                    var descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(opts =>
                        opts.UseNpgsql(_postgres.GetConnectionString()));
                });
            });

        // Apply migrations
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await _postgres.DisposeAsync();
    }
}
