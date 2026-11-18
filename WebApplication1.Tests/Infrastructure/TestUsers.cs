using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.models;

namespace WebApplication1.Tests.Infrastructure;

public static class TestUsers
{
    public static async Task<HttpClient> LoginAsAsync(
        CustomWebApplicationFactory factory,
        string role = "Gast")
    {
        var client = factory.CreateClient();

        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "TestAccount!2026";

        var registerResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        if (role != "Gast")
        {
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            var user = await userManager.FindByEmailAsync(email);

            Assert.NotNull(user);

            var result = await userManager.AddToRoleAsync(
                user, role);

            Assert.True(result.Succeeded);
        }

        var loginResponse = await client.PostAsJsonAsync(
            "/auth/login",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        return client;
    }
}
