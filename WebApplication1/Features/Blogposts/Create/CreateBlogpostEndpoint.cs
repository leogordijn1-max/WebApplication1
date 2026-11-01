namespace WebApplication1.Features.Blogposts.Create;

public static class CreateBlogpostEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/blogposts", CreateBlogpostHandler.Handle);
    }
}
