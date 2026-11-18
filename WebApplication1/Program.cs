using Microsoft.AspNetCore.Identity;
using WebApplication1.models;
using Microsoft.EntityFrameworkCore;
using WebApplication1.data;
using WebApplication1.Features.Blogposts.Create;
using WebApplication1.Features.Blogposts.Delete;
using WebApplication1.Features.Blogposts.GetAll;
using WebApplication1.Features.Blogposts.GetById;
using WebApplication1.Features.Blogposts.Update;
using WebApplication1.Features.Projects.Create;
using WebApplication1.Features.Projects.Delete;
using WebApplication1.Features.Projects.GetAll;
using WebApplication1.Features.Projects.GetById;
using WebApplication1.Features.Projects.Update;
using WebApplication1.Features.Auth.Register;
using WebApplication1.data.Seed;
using WebApplication1.Features.Auth.Login;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});


builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditPortfolio", policy =>
        policy.RequireRole("Editor", "Admin"));

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// PROJECTS //

GetAllProjectsEndpoint.MapEndpoint(app);
GetProjectByIdEndpoint.MapEndpoint(app);
CreateProjectEndpoint.MapEndpoint(app);
UpdateProjectEndpoint.MapEndpoint(app);
DeleteProjectEndpoint.MapEndpoint(app);

// BLOGPOSTS //
GetAllBlogpostsEndpoint.MapEndpoint(app);
GetBlogpostByIdEndpoint.MapEndpoint(app);
CreateBlogpostEndpoint.MapEndpoint(app);
UpdateBlogpostEndpoint.MapEndpoint(app);
DeleteBlogpostEndpoint.MapEndpoint(app);

// Authentication //
RegisterEndpoint.MapEndpoint(app);

// Login //
LoginEndpoint.MapEndpoint(app);


if (app.Environment.IsEnvironment("Testing"))
{
using var scope = app.Services.CreateScope();

var db = scope.ServiceProvider
    .GetRequiredService<AppDbContext>();

await db.Database.EnsureCreatedAsync();
}

await RoleSeeder.SeedAsync(app.Services);

app.Run();

public partial class Program
{
}
