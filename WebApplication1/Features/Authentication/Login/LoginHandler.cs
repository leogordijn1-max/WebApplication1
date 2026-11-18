
using Microsoft.AspNetCore.Identity;
using WebApplication1.models;

namespace WebApplication1.Features.Auth.Login;

public static class LoginHandler
{
    public static async Task<IResult> Handle(
        LoginDto dto,
        SignInManager<User> signInManager)
    {
        var result = await signInManager.PasswordSignInAsync(
            dto.Email,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Message = "Succesvol ingelogd"
        });
    }
}
