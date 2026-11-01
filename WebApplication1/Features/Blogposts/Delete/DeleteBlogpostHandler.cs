using WebApplication1.data;

namespace WebApplication1.Features.Blogposts.Delete;

public static class DeleteBlogpostHandler
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var blogpost = await db.Blogposts.FindAsync(id);

        if (blogpost == null)
            return Results.NotFound();

        db.Blogposts.Remove(blogpost);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
