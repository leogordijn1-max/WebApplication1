using WebApplication1.data;

namespace WebApplication1.Features.Projects.GetById;

public static class GetProjectByIdHandler
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
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
    }
}
