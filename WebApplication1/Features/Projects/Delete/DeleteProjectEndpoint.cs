namespace WebApplication1.Features.Projects.Delete;

public static class DeleteProjectEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/projects/{id}", DeleteProjectHandler.Handle);
    }
}
