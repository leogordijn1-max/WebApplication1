using Microsoft.EntityFrameworkCore;
using WebApplication1.data;

namespace WebApplication1.Features.Projects.GetAll;

public static class GetAllProjectsHandler
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var projecten = await db.Projects.ToListAsync();

        var dtos = projecten.Select(project => new ProjectDto
        {
            Id = project.Id,
            Titel = project.Titel,
            Beschrijving = project.Beschrijving,
            Categorie = project.Categorie,
            GitHubUrl = project.GitHubUrl,
            Datum = project.Datum
        }).ToList();

        return Results.Ok(dtos);
    }
}
