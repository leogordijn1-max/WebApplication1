using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.data;
using WebApplication1.models;
using WebApplication1.Features.Blogposts;
using WebApplication1.Tests.Infrastructure;

namespace WebApplication1.Tests.Blogposts;

public class BlogpostEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BlogpostEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Testgegevens maken zonder HTTP-aanroep.
    private async Task<int> MaakBlogpostAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var blogpost = new Blogpost
        {
            Titel = $"Testblog {Guid.NewGuid()}",
            Inhoud = "Inhoud van de testblog",
            Publicatiedatum = new DateTime(2026, 11, 18)
        };

        db.Blogposts.Add(blogpost);
        await db.SaveChangesAsync();

        return blogpost.Id;
    }

    private static object GeldigeBlogpost()
    {
        return new
        {
            titel = "Een nieuwe blogpost",
            inhoud = "Dit is de inhoud van de blogpost.",
            publicatiedatum = "2026-11-18"
        };
    }

    // GET ALL

    [Fact]
    public async Task GetAllBlogposts_ZonderLogin_Geeft200()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/blogposts");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // GET BY ID

    [Fact]
    public async Task GetBlogpostById_Bestaand_Geeft200()
    {
        var id = await MaakBlogpostAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/blogposts/{id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<BlogpostDto>();

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetBlogpostById_Onbekend_Geeft404()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(
            "/blogposts/2147483647");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // CREATE

    [Fact]
    public async Task CreateBlogpost_AlsEditor_WordtOpgeslagen()
    {
        using var client = await TestUsers.LoginAsAsync(
            _factory, "Editor");

        var response = await client.PostAsJsonAsync(
            "/blogposts", GeldigeBlogpost());

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<BlogpostDto>();

        Assert.NotNull(result);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var opgeslagen = await db.Blogposts.FindAsync(result.Id);

        Assert.NotNull(opgeslagen);
        Assert.Equal("Een nieuwe blogpost", opgeslagen.Titel);
    }

    // UPDATE

    [Fact]
    public async Task UpdateBlogpost_AlsEditor_WordtOpgeslagen()
    {
        var id = await MaakBlogpostAsync();

        using var client = await TestUsers.LoginAsAsync(
            _factory, "Editor");

        var response = await client.PutAsJsonAsync(
            $"/blogposts/{id}",
            new
            {
                titel = "Gewijzigde titel",
                inhoud = "Gewijzigde inhoud",
                publicatiedatum = "2026-11-19"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var gewijzigd = await db.Blogposts.FindAsync(id);

        Assert.NotNull(gewijzigd);
        Assert.Equal("Gewijzigde titel", gewijzigd.Titel);
        Assert.Equal("Gewijzigde inhoud", gewijzigd.Inhoud);
    }

    // DELETE

    [Fact]
    public async Task DeleteBlogpost_AlsAdmin_WordtVerwijderd()
    {
        var id = await MaakBlogpostAsync();

        using var client = await TestUsers.LoginAsAsync(
            _factory, "Admin");

        var response = await client.DeleteAsync(
            $"/blogposts/{id}");

        Assert.True(response.IsSuccessStatusCode);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var verwijderd = await db.Blogposts.FindAsync(id);

        Assert.Null(verwijderd);
    }

    // ONBEKENDE ID'S

    [Fact]
    public async Task UpdateBlogpost_OnbekendId_Geeft404()
    {
        using var client = await TestUsers.LoginAsAsync(
            _factory, "Editor");

        var response = await client.PutAsJsonAsync(
            "/blogposts/2147483647",
            GeldigeBlogpost());

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task DeleteBlogpost_OnbekendId_Geeft404()
    {
        using var client = await TestUsers.LoginAsAsync(
            _factory, "Editor");

        var response = await client.DeleteAsync(
            "/blogposts/2147483647");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // ONGELDIGE INVOER

    [Fact]
    public async Task CreateBlogpost_Ongeldig_Geeft400()
    {
        using var client = await TestUsers.LoginAsAsync(
            _factory, "Editor");

        var response = await client.PostAsJsonAsync(
            "/blogposts",
            new
            {
                titel = "",
                inhoud = "",
                publicatiedatum = "2026-11-18"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateBlogpost_Ongeldig_Geeft400()
    {
        var id = await MaakBlogpostAsync();

        using var client = await TestUsers.LoginAsAsync(
            _factory, "Editor");

        var response = await client.PutAsJsonAsync(
            $"/blogposts/{id}",
            new
            {
                titel = "",
                inhoud = "",
                publicatiedatum = "2026-11-18"
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // 401: NIET INGELOGD

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Schrijven_ZonderLogin_Geeft401(
        string methode)
    {
        using var client = _factory.CreateClient();

        HttpResponseMessage response = methode switch
        {
            "POST" => await client.PostAsJsonAsync(
                "/blogposts", GeldigeBlogpost()),

            "PUT" => await client.PutAsJsonAsync(
                "/blogposts/2147483647",
                GeldigeBlogpost()),

            _ => await client.DeleteAsync(
                "/blogposts/2147483647")
        };

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // 403: INGELOGD ALS GAST

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Schrijven_AlsGast_Geeft403(
        string methode)
    {
        using var client = await TestUsers.LoginAsAsync(
            _factory, "Gast");

        HttpResponseMessage response = methode switch
        {
            "POST" => await client.PostAsJsonAsync(
                "/blogposts", GeldigeBlogpost()),

            "PUT" => await client.PutAsJsonAsync(
                "/blogposts/2147483647",
                GeldigeBlogpost()),

            _ => await client.DeleteAsync(
                "/blogposts/2147483647")
        };

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}
