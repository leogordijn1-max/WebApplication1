namespace WebApplication1.Features.Projects.Create;

public static class CreateProjectEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/projects", CreateProjectHandler.Handle);
    }
}
