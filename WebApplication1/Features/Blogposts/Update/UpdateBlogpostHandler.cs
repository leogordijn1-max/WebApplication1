using WebApplication1.data;

namespace WebApplication1.Features.Blogposts.Update;

public static class UpdateBlogpostHandler
{
    public static async Task<IResult> Handle(
        int id, UpdateBlogpostDto dto, AppDbContext db)
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
    }
}
