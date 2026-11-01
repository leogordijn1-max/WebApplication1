namespace WebApplication1.Features.Blogposts.Update;

public static class UpdateBlogpostEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPut("/blogposts/{id}", UpdateBlogpostHandler.Handle);
    }
}
