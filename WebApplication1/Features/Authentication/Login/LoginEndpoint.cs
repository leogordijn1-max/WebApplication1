namespace WebApplication1.Features.Auth.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/auth/login", LoginHandler.Handle)
            .AllowAnonymous();
    }
}