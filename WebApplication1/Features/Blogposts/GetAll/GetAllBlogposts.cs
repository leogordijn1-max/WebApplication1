
using Microsoft.EntityFrameworkCore;
using WebApplication1.data;
using WebApplication1.DTOs;

namespace WebApplication1.Features.Blogposts.GetAll;

public static class GetAllBlogposts
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/blogposts", async (AppDbContext db) =>
        {
            var blogposts = await db.Blogposts.ToListAsync();

            var dtos = blogposts.Select(blogpost => new BlogpostDto
            {
                Id = blogpost.Id,
                Titel = blogpost.Titel,
                Inhoud = blogpost.Inhoud,
                Publicatiedatum = blogpost.Publicatiedatum
            }).ToList();

            return Results.Ok(dtos);
        });
    }
}
