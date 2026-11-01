namespace WebApplication1.Features.Blogposts.GetById;

public static class GetBlogpostByIdEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/blogposts/{id}", GetBlogpostByIdHandler.Handle);
    }
}
