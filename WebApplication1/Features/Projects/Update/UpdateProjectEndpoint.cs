
namespace WebApplication1.Features.Projects.Update;

public static class UpdateProjectEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPut("/projects/{id}", UpdateProjectHandler.Handle)
            .RequireAuthorization("CanEditPortfolio");
    }
}
