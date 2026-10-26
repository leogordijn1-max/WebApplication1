
using WebApplication1.data;
using WebApplication1.DTOs;
using WebApplication1.models;

namespace WebApplication1.Features.Blogposts.Create;

public static class CreateBlogpost
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/blogposts", async (CreateBlogpostDto dto, AppDbContext db) =>
        {
            var blogpost = new Blogpost
            {
                Titel = dto.Titel,
                Inhoud = dto.Inhoud,
                Publicatiedatum = dto.Publicatiedatum
            };

            db.Blogposts.Add(blogpost);
            await db.SaveChangesAsync();

            var resultaat = new BlogpostDto
            {
                Id = blogpost.Id,
                Titel = blogpost.Titel,
                Inhoud = blogpost.Inhoud,
                Publicatiedatum = blogpost.Publicatiedatum
            };

            return Results.Created($"/blogposts/{blogpost.Id}", resultaat);
        });
    }
}

