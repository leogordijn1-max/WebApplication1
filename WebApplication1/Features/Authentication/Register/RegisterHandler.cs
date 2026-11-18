
using Microsoft.AspNetCore.Identity;
using WebApplication1.models;

namespace WebApplication1.Features.Auth.Register;

public static class RegisterHandler
{
    public static async Task<IResult> Handle(
        RegisterDto dto,
        UserManager<User> userManager)
    {
        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "Gast");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Results.Problem(
                "De rol Gast kon niet worden toegewezen.");
        }

        return Results.Created(
            $"/auth/users/{user.Id}",
            new { user.Id, user.Email, Role = "Gast" });
    }
}
