using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WebApplication1.data;
using WebApplication1.Features.Projects;
using WebApplication1.models;
using WebApplication1.Tests.Infrastructure;

namespace WebApplication1.Tests.Projects;

public class ProjectEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProjectEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    private async Task LoginAlsEditorAsync()
    {
        var email = $"editor-{Guid.NewGuid()}@example.com";
        var password = "TestEditor!2026";

        var registerResponse = await _client.PostAsJsonAsync(
            "/auth/register",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            var user = await userManager.FindByEmailAsync(email);

            Assert.NotNull(user);

            var result = await userManager.AddToRoleAsync(
                user, "Editor");

            Assert.True(result.Succeeded);
        }

        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ZonderLogin_Geeft200()
    {
        // Arrange
        // Geen gebruiker ingelogd.

        // Act
        var response = await _client.GetAsync("/projects");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_ZonderLogin_Geeft401()
    {
        // Arrange
        // We zijn niet ingelogd.
        var project = new
        {
            titel = "Testproject",
            beschrijving = "Dit is een testproject",
            categorie = "School",
            gitHubUrl = "https://github.com/test",
            datum = "2026-11-16"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/projects", project);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_AlsGast_Geeft403()
    {
        // Arrange: een nieuw Gast-account registreren.
        var email = $"gast-{Guid.NewGuid()}@example.com";
        var password = "TestGast!2026";

        var registerResponse = await _client.PostAsJsonAsync(
            "/auth/register",
            new
            {
                email,
                password
            });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        // Inloggen met het aangemaakte account.
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                email,
                password
            });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        // Act: als Gast proberen een project aan te maken.
        var response = await _client.PostAsJsonAsync(
            "/projects",
            new
            {
                titel = "Verboden project",
                beschrijving = "Een Gast mag dit niet toevoegen",
                categorie = "School",
                gitHubUrl = "https://github.com/test",
                datum = "2026-11-16"
            });

        // Assert: ingelogd, maar onvoldoende rechten.
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_AlsEditor_WordtOpgeslagen()
    {
        // Arrange: een nieuwe gebruiker registreren.
        var email = $"editor-{Guid.NewGuid()}@example.com";
        var password = "TestEditor!2026";

        var registerResponse = await _client.PostAsJsonAsync(
            "/auth/register",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        // De gebruiker Editor-rechten geven via Identity.
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            var user = await userManager.FindByEmailAsync(email);

            Assert.NotNull(user);

            var roleResult = await userManager.AddToRoleAsync(
                user, "Editor");

            Assert.True(roleResult.Succeeded);
        }

        // Inloggen na de rolwijziging.
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var project = new
        {
            titel = "Automatische Editor Test",
            beschrijving = "Aangemaakt vanuit xUnit",
            categorie = "School",
            gitHubUrl = "https://github.com/test",
            datum = "2026-11-16"
        };

        // Act: een project aanmaken als Editor.
        var response = await _client.PostAsJsonAsync(
            "/projects", project);

        // Assert: de API accepteert het verzoek.
        Assert.True(
            response.IsSuccessStatusCode,
            $"Onverwachte statuscode: {response.StatusCode}");

        // Assert: het project staat echt in de testdatabase.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var savedProject = db.Projects.SingleOrDefault(
                p => p.Titel == "Automatische Editor Test");

            Assert.NotNull(savedProject);
            Assert.Equal("School", savedProject.Categorie);
            Assert.Equal(
                "Aangemaakt vanuit xUnit",
                savedProject.Beschrijving);
        }
    }

    [Fact]
    public async Task GetProjectById_BestaandProject_Geeft200()
    {
        // Arrange: een project toevoegen aan de testdatabase.
        int projectId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var project = new Project
            {
                Titel = "Project voor GET-test",
                Beschrijving = "Test voor ophalen op ID",
                Categorie = "School",
                GitHubUrl = "https://github.com/test",
                Datum = new DateTime(2026, 11, 16)
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            projectId = project.Id;
        }

        // Act
        var response = await _client.GetAsync(
            $"/projects/{projectId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(result);
        Assert.Equal(projectId, result.Id);
        Assert.Equal("Project voor GET-test", result.Titel);
    }

    [Fact]
    public async Task GetProjectById_OnbekendProject_Geeft404()
    {
        // Act: een ID gebruiken dat niet bestaat.
        var response = await _client.GetAsync(
            "/projects/2147483647");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateProject_AlsEditor_WijzigingenOpgeslagen()
    {
        // Arrange: een bestaand project aanmaken.
        int projectId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var project = new Project
            {
                Titel = "Oude titel",
                Beschrijving = "Oude beschrijving",
                Categorie = "School",
                GitHubUrl = "https://github.com/oud",
                Datum = new DateTime(2026, 11, 16)
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            projectId = project.Id;
        }

        // Arrange: een Editor-account maken.
        var email = $"editor-{Guid.NewGuid()}@example.com";
        var password = "TestEditor!2026";

        var registerResponse = await _client.PostAsJsonAsync(
            "/auth/register", new { email, password });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);

            var roleResult = await userManager.AddToRoleAsync(user, "Editor");
            Assert.True(roleResult.Succeeded);
        }

        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login", new { email, password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Act: het bestaande project wijzigen.
        var response = await _client.PutAsJsonAsync(
            $"/projects/{projectId}",
            new
            {
                titel = "Nieuwe titel",
                beschrijving = "Nieuwe beschrijving",
                categorie = "Project",
                gitHubUrl = "https://github.com/nieuw",
                datum = "2026-11-17"
            });

        // Assert: HTTP-verzoek is geslaagd.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert: wijzigingen staan werkelijk in de database.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var updatedProject = await db.Projects.FindAsync(projectId);

            Assert.NotNull(updatedProject);
            Assert.Equal("Nieuwe titel", updatedProject.Titel);
            Assert.Equal("Nieuwe beschrijving", updatedProject.Beschrijving);
            Assert.Equal("Project", updatedProject.Categorie);
        }
    }

    [Fact]
    public async Task DeleteProject_AlsEditor_VerwijdertProject()
    {
        // Arrange: een project toevoegen aan de testdatabase.
        int projectId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var project = new Project
            {
                Titel = "Project om te verwijderen",
                Beschrijving = "Dit project wordt verwijderd",
                Categorie = "School",
                GitHubUrl = "https://github.com/test",
                Datum = new DateTime(2026, 11, 16)
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            projectId = project.Id;
        }

        // Arrange: een Editor-account maken.
        var email = $"editor-{Guid.NewGuid()}@example.com";
        var password = "TestEditor!2026";

        var registerResponse = await _client.PostAsJsonAsync(
            "/auth/register", new { email, password });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);

            var roleResult = await userManager.AddToRoleAsync(user, "Editor");
            Assert.True(roleResult.Succeeded);
        }

        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login", new { email, password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Act: het project verwijderen.
        var response = await _client.DeleteAsync(
            $"/projects/{projectId}");

        // Assert: de API accepteert de verwijdering.
        Assert.True(
            response.IsSuccessStatusCode,
            $"Onverwachte statuscode: {response.StatusCode}");

        // Assert: het project bestaat niet meer.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var deletedProject = await db.Projects.FindAsync(projectId);

            Assert.Null(deletedProject);
        }
    }


    [Fact]
    public async Task UpdateProject_OnbekendId_Geeft404()
    {
        // Arrange
        await LoginAlsEditorAsync();

        var updatedProject = new
        {
            titel = "Nieuwe titel",
            beschrijving = "Nieuwe beschrijving",
            categorie = "School",
            gitHubUrl = "https://github.com/test",
            datum = "2026-11-16"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            "/projects/2147483647",
            updatedProject);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_OnbekendId_Geeft404()
    {
        // Arrange
        await LoginAlsEditorAsync();

        // Act
        var response = await _client.DeleteAsync(
            "/projects/2147483647");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task ProjectWijzigen_ZonderLogin_Geeft401(
        string methode)
    {
        // Arrange: geen gebruiker ingelogd.
        using var client = _factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

        // Act
        HttpResponseMessage response;

        if (methode == "PUT")
        {
            response = await client.PutAsJsonAsync(
                "/projects/2147483647",
                new
                {
                    titel = "Testproject",
                    beschrijving = "Testbeschrijving",
                    categorie = "School",
                    gitHubUrl = "https://github.com/test",
                    datum = "2026-11-16"
                });
        }
        else
        {
            response = await client.DeleteAsync(
                "/projects/2147483647");
        }

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task ProjectWijzigen_AlsGast_Geeft403(
        string methode)
    {
        // Arrange: een nieuw Gast-account registreren.
        using var client = _factory.CreateClient();

        var email = $"gast-{Guid.NewGuid()}@example.com";
        var password = "TestGast!2026";

        var registerResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        // Inloggen als Gast.
        var loginResponse = await client.PostAsJsonAsync(
            "/auth/login",
            new { email, password });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        // Act: proberen een project te wijzigen of verwijderen.
        HttpResponseMessage response;

        if (methode == "PUT")
        {
            response = await client.PutAsJsonAsync(
                "/projects/2147483647",
                new
                {
                    titel = "Verboden wijziging",
                    beschrijving = "Een Gast mag dit niet",
                    categorie = "School",
                    gitHubUrl = "https://github.com/test",
                    datum = "2026-11-16"
                });
        }
        else
        {
            response = await client.DeleteAsync(
                "/projects/2147483647");
        }

        // Assert: ingelogd, maar onvoldoende rechten.
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_MetOngeldigeInvoer_WordtGeweigerd()
    {
        // Arrange: inloggen als Editor.
        await LoginAlsEditorAsync();

        var ongeldigProject = new
        {
            titel = "",
            beschrijving = "",
            categorie = "",
            gitHubUrl = "geen-geldige-url",
            datum = "2026-11-16"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/projects",
            ongeldigProject);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task CreateProject_AlsAdmin_WordtOpgeslagen()
    {
        using var client = await TestUsers.LoginAsAsync(
            _factory, "Admin");

        var titel = $"Adminproject-{Guid.NewGuid()}";

        var response = await client.PostAsJsonAsync(
            "/projects",
            new
            {
                titel,
                beschrijving = "Aangemaakt door een Admin",
                categorie = "School",
                gitHubUrl = "https://github.com/test",
                datum = "2026-11-18"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.Contains(
            db.Projects,
            project => project.Titel == titel);
    }

    [Fact]
    public async Task UpdateProject_OngeldigeInvoer_Geeft400()
    {
        // Arrange: een project in de testdatabase.
        int id;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var project = new Project
            {
                Titel = "Originele titel",
                Beschrijving = "Originele beschrijving",
                Categorie = "School",
                GitHubUrl = "https://github.com/test",
                Datum = new DateTime(2026, 11, 18)
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            id = project.Id;
        }

        using var client = await TestUsers.LoginAsAsync(
            _factory, "Editor");

        // Act
        var response = await client.PutAsJsonAsync(
            $"/projects/{id}",
            new
            {
                titel = "",
                beschrijving = "",
                categorie = "",
                gitHubUrl = "ongeldige-url",
                datum = "2026-11-18"
            });

        // Assert: invoer wordt geweigerd.
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        // Assert: oorspronkelijke gegevens blijven staan.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var opgeslagen = await db.Projects.FindAsync(id);

            Assert.NotNull(opgeslagen);
            Assert.Equal("Originele titel", opgeslagen.Titel);
        }
    }


}
