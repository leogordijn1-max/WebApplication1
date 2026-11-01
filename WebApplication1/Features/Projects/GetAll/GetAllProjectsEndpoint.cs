namespace WebApplication1.Features.Projects.GetAll;

public static class GetAllProjectsEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/projects", GetAllProjectsHandler.Handle);
    }
}
