namespace WebApplication1.Features.Projects.GetById;

public static class GetProjectByIdEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/projects/{id}", GetProjectByIdHandler.Handle);
    }
}