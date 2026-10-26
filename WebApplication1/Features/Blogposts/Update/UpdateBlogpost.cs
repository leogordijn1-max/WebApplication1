
using WebApplication1.data;
using WebApplication1.DTOs;

namespace WebApplication1.Features.Blogposts.Update;

public static class UpdateBlogpost
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPut("/blogposts/{id}", async (int id, UpdateBlogpostDto dto, AppDbContext db) =>
        {
            var blogpost = await db.Blogposts.FindAsync(id);

            if (blogpost == null)
                return Results.NotFound();

            blogpost.Titel = dto.Titel;
            blogpost.Inhoud = dto.Inhoud;
            blogpost.Publicatiedatum = dto.Publicatiedatum;

            await db.SaveChangesAsync();

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
