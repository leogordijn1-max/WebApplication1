
using WebApplication1.data;
using WebApplication1.DTOs;

namespace WebApplication1.Features.Projects.GetById;

public static class GetProjectById
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/projects/{id}", async (int id, AppDbContext db) =>
        {
            var project = await db.Projects.FindAsync(id);

            if (project == null)
                return Results.NotFound();

            return Results.Ok(new ProjectDto
            {
                Id = project.Id,
                Titel = project.Titel,
                Beschrijving = project.Beschrijving,
                Categorie = project.Categorie,
                GitHubUrl = project.GitHubUrl,
                Datum = project.Datum
            });
        });
    }
}
