
using WebApplication1.data;

namespace WebApplication1.Features.Projects.Delete;

public static class DeleteProject
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/projects/{id}", async (int id, AppDbContext db) =>
        {
            var project = await db.Projects.FindAsync(id);

            if (project == null)
                return Results.NotFound();

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
