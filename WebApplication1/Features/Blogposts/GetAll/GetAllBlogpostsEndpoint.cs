namespace WebApplication1.Features.Blogposts.GetAll;

public static class GetAllBlogpostsEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/blogposts", GetAllBlogpostsHandler.Handle);
    }
}

