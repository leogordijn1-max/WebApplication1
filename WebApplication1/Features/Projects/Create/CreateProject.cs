
using WebApplication1.data;
using WebApplication1.DTOs;
using WebApplication1.models;

namespace WebApplication1.Features.Projects.Create;

public static class CreateProject
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/projects", async (CreateProjectDto dto, AppDbContext db) =>
        {
            var project = new Project
            {
                Titel = dto.Titel,
                Beschrijving = dto.Beschrijving,
                Categorie = dto.Categorie,
                GitHubUrl = dto.GitHubUrl,
                Datum = dto.Datum
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            var resultaat = new ProjectDto
            {
                Id = project.Id,
                Titel = project.Titel,
                Beschrijving = project.Beschrijving,
                Categorie = project.Categorie,
                GitHubUrl = project.GitHubUrl,
                Datum = project.Datum
            };

            return Results.Created($"/projects/{project.Id}", resultaat);
        });
    }
}
