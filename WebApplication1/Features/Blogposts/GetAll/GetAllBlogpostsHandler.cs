using Microsoft.EntityFrameworkCore;
using WebApplication1.data;

namespace WebApplication1.Features.Blogposts.GetAll;

public static class GetAllBlogpostsHandler
{
    public static async Task<IResult> Handle(AppDbContext db)
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
    }
}
