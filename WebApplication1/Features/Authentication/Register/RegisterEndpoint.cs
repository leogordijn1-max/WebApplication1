namespace WebApplication1.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/auth/register", RegisterHandler.Handle)
            .AllowAnonymous();
    }
}