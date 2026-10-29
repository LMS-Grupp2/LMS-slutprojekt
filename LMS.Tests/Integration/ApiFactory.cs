using LMS.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LMS.Tests.Integration;

/// <summary>
/// Starts LMS.API in memory with its own JWT settings. The database is never opened: the seed
/// service is removed, and the tests only call endpoints where the request is stopped before a
/// controller runs (or, for api/auth/me, a controller that only reads the token's claims).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string Issuer = "LMS.Tests";
    public const string Audience = "LMS.Tests";

    // HMAC-SHA256 needs a key of at least 32 bytes. Only valid inside this test server.
    public const string SecretKey = "test-secret-key-that-is-at-least-32-bytes-long!!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs throws without a connection string, but nothing here connects to it
        builder.UseSetting("ConnectionStrings:ApplicationDbContext", "Server=unused;Database=unused;");
        builder.UseSetting("ActiveConnection", "");

        // Our own JWT settings, so the tests never depend on a developer's user secrets (or CI having none)
        builder.UseSetting("JwtSettings:Issuer", Issuer);
        builder.UseSetting("JwtSettings:Audience", Audience);
        builder.UseSetting("JwtSettings:Expires", "5");
        builder.UseSetting("JwtSettings:SecretKey", SecretKey);

        builder.ConfigureTestServices(services =>
        {
            // The seed is a hosted service that opens the database at startup. Remove only that one:
            // the test server itself is also registered as a hosted service.
            var seed = services.Single(d =>
                d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(DataSeedService));
            services.Remove(seed);
        });
    }
}
