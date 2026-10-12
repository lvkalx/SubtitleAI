using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SubtitleAI.IntegrationTests;

public sealed class ApiSecurityTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiSecurityTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Live_health_endpoint_has_security_headers()
    {
        var response = await _client.GetAsync("/health/live");

        response.EnsureSuccessStatusCode();
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("no-referrer");
    }

    [Fact]
    public async Task Jobs_list_requires_an_access_token()
    {
        var response = await _client.GetAsync("/api/jobs?page=1&pageSize=20");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public ApiFactory()
    {
        // Minimal APIs are bootstrapped before WebApplicationFactory's in-memory
        // configuration hook; environment variables are available during bootstrap.
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", "Host=localhost;Port=5432;Database=subtitleai_test;Username=test;Password=test");
        Environment.SetEnvironmentVariable("Jwt__Key", "test-signing-key-that-is-long-enough-for-hmac-sha256");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "SubtitleAI.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "SubtitleAI.Tests");
        Environment.SetEnvironmentVariable("Storage__ConnectionString", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable("Storage__Container", "media");
        Environment.SetEnvironmentVariable("Database__MigrateOnStartup", "false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Postgres", "Host=localhost;Port=5432;Database=subtitleai_test;Username=test;Password=test");
        builder.UseSetting("Jwt:Key", "test-signing-key-that-is-long-enough-for-hmac-sha256");
        builder.UseSetting("Jwt:Issuer", "SubtitleAI.Tests");
        builder.UseSetting("Jwt:Audience", "SubtitleAI.Tests");
        builder.UseSetting("Storage:ConnectionString", "UseDevelopmentStorage=true");
        builder.UseSetting("Storage:Container", "media");
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Postgres"] = "Host=localhost;Port=5432;Database=subtitleai_test;Username=test;Password=test",
            ["Jwt:Key"] = "test-signing-key-that-is-long-enough-for-hmac-sha256",
            ["Jwt:Issuer"] = "SubtitleAI.Tests",
            ["Jwt:Audience"] = "SubtitleAI.Tests",
            ["Storage:ConnectionString"] = "UseDevelopmentStorage=true",
            ["Storage:Container"] = "media",
            ["Database:MigrateOnStartup"] = "false"
        }));
    }
}
