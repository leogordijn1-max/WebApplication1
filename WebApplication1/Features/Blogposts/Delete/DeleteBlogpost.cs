
using WebApplication1.data;

namespace WebApplication1.Features.Blogposts.Delete;

public static class DeleteBlogpost
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/blogposts/{id}", async (int id, AppDbContext db) =>
        {
            var blogpost = await db.Blogposts.FindAsync(id);

            if (blogpost == null)
                return Results.NotFound();

            db.Blogposts.Remove(blogpost);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
