using WebApplication1.data;

namespace WebApplication1.Features.Blogposts.GetById;

public static class GetBlogpostByIdHandler
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
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
    }
}
