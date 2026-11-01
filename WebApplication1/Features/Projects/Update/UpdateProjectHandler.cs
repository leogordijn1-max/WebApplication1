using WebApplication1.data;

namespace WebApplication1.Features.Projects.Update;

public static class UpdateProjectHandler
{
    public static async Task<IResult> Handle(
        int id, UpdateProjectDto dto, AppDbContext db)
    {
        var project = await db.Projects.FindAsync(id);

        if (project == null)
            return Results.NotFound();

        project.Titel = dto.Titel;
        project.Beschrijving = dto.Beschrijving;
        project.Categorie = dto.Categorie;
        project.GitHubUrl = dto.GitHubUrl;
        project.Datum = dto.Datum;

        await db.SaveChangesAsync();

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
