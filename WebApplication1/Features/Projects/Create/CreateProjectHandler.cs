using WebApplication1.data;
using WebApplication1.models;

namespace WebApplication1.Features.Projects.Create;

public static class CreateProjectHandler
{
    public static async Task<IResult> Handle(
        CreateProjectDto dto,
        AppDbContext db)
    {
        var project = new Project
        {
            Titel = dto.Titel,
            Beschrijving = dto.Beschrijving,
            Categorie = dto.Categorie,
            GitHubUrl = dto.GitHubUrl,
            Datum = dto.Datum
        };


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
    }
}
