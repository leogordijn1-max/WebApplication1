using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.models;
using WebApplication1.Tests.Infrastructure;

namespace WebApplication1.Tests.Auth;

public class AuthEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_NieuweGebruiker_KrijgtRolGast()
    {
        using var client = _factory.CreateClient();

        var email = $"gast-{Guid.NewGuid()}@example.com";

        var response = await client.PostAsJsonAsync(
            "/auth/register",
            new
            {
                email,
                password = "TestAccount!2026"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<User>>();

        var user = await userManager.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.True(
            await userManager.IsInRoleAsync(user, "Gast"));

        Assert.False(
            await userManager.IsInRoleAsync(user, "Editor"));

        Assert.False(
            await userManager.IsInRoleAsync(user, "Admin"));
    }

    [Fact]
    public async Task Register_DubbelEmail_WordtGeweigerd()
    {
        using var client = _factory.CreateClient();

        var account = new
        {
            email = $"dubbel-{Guid.NewGuid()}@example.com",
            password = "TestAccount!2026"
        };

        var eerste = await client.PostAsJsonAsync(
            "/auth/register", account);

        var tweede = await client.PostAsJsonAsync(
            "/auth/register", account);

        Assert.Equal(
            HttpStatusCode.Created,
            eerste.StatusCode);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            tweede.StatusCode);
    }

    [Fact]
    public async Task Register_ZwakWachtwoord_Geeft400()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/auth/register",
            new
            {
                email = $"zwak-{Guid.NewGuid()}@example.com",
                password = "123"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_JuisteGegevens_GeeftCookie()
    {
        using var client = _factory.CreateClient();

        var email = $"login-{Guid.NewGuid()}@example.com";
        var password = "TestAccount!2026";

        var registerResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.True(
            response.Headers.TryGetValues(
                "Set-Cookie", out var cookies));

        Assert.Contains(
            cookies!,
            cookie => cookie.Contains(
                ".AspNetCore.Identity.Application"));
    }

    [Fact]
    public async Task Login_VerkeerdWachtwoord_Geeft401()
    {
        using var client = _factory.CreateClient();

        var email = $"login-{Guid.NewGuid()}@example.com";

        var registerResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new
            {
                email,
                password = "TestAccount!2026"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                email,
                password = "Verkeerd!2026"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_OnbekendeGebruiker_Geeft401()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                email = $"onbekend-{Guid.NewGuid()}@example.com",
                password = "TestAccount!2026"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
