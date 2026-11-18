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

        if (string.IsNullOrWhiteSpace(dto.Titel) ||
    string.IsNullOrWhiteSpace(dto.Beschrijving) ||
    string.IsNullOrWhiteSpace(dto.Categorie))
        {
            return Results.BadRequest(
                "Titel, beschrijving en categorie zijn verplicht.");
        }

        if (!Uri.TryCreate(dto.GitHubUrl, UriKind.Absolute, out var url) ||
            (url.Scheme != Uri.UriSchemeHttp &&
             url.Scheme != Uri.UriSchemeHttps))
        {
            return Results.BadRequest(
                "GitHubUrl moet een geldige HTTP- of HTTPS-URL zijn.");
        }

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
