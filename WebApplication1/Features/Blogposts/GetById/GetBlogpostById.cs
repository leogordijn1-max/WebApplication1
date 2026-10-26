
using WebApplication1.data;
using WebApplication1.DTOs;

namespace WebApplication1.Features.Blogposts.GetById;

public static class GetBlogpostById
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/blogposts/{id}", async (int id, AppDbContext db) =>
        {
            var blogpost = await db.Blogposts.FindAsync(id);

            if (blogpost == null)
                return Results.NotFound();

            return Results.Ok(new BlogpostDto
            {
                Id = blogpost.Id,
                Titel = blogpost.Titel,
                Inhoud = blogpost.Inhoud,
                Publicatiedatum = blogpost.Publicatiedatum
            });
        });
    }
}

