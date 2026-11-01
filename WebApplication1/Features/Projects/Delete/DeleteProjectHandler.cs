using WebApplication1.data;

namespace WebApplication1.Features.Projects.Delete;

public static class DeleteProjectHandler
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var project = await db.Projects.FindAsync(id);

        if (project == null)
            return Results.NotFound();

        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
