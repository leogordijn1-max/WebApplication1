namespace WebApplication1.Features.Blogposts.Delete;

public static class DeleteBlogpostEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/blogposts/{id}", DeleteBlogpostHandler.Handle);
    }
}
