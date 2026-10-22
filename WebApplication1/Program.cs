using WebApplication1.models;
using Microsoft.EntityFrameworkCore;
using WebApplication1.data;
using WebApplication1.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

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

app.UseAuthorization();

app.MapControllers();

// GET //

app.MapGet("/projects", async (AppDbContext db) =>
{
    return await db.Projects.ToListAsync();
});

app.MapGet("/projects/{id}", async (int id, AppDbContext db) =>
{
    var project = await db.Projects.FindAsync(id);

    if (project == null)
    {
        return Results.NotFound();
    }

    var dto = new ProjectDto
    {
        Id = project.Id,
        Titel = project.Titel,
        Beschrijving = project.Beschrijving,
        Categorie = project.Categorie,
        GitHubUrl = project.GitHubUrl,
        Datum = project.Datum
    };

    return Results.Ok(dto);
});
// POST //

app.MapPost("/projects", async (CreateProjectDto dto, AppDbContext db) =>
{
    var project = new Project
    {
        Titel = dto.Titel,
        Beschrijving = dto.Beschrijving,
        Categorie = dto.Categorie,
        GitHubUrl = dto.GitHubUrl,
        Datum = dto.Datum
    };

    db.Projects.Add(project);

    await db.SaveChangesAsync();

    return Results.Created($"/projects/{project.Id}", project);
});

// PUT //

app.MapPut("/projects/{id}", async (int id, Project aangepastProject, AppDbContext db) =>
{
    var project = await db.Projects.FindAsync(id);

    if (project == null)
    {
        return Results.NotFound();
    }

    project.Titel = aangepastProject.Titel;
    project.Beschrijving = aangepastProject.Beschrijving;
    project.Categorie = aangepastProject.Categorie;
    project.GitHubUrl = aangepastProject.GitHubUrl;
    project.Datum = aangepastProject.Datum;

    await db.SaveChangesAsync();

    return Results.Ok(project);
});

// DELETE //

app.MapDelete("/projects/{id}", async (int id, AppDbContext db) =>
{
    var project = await db.Projects.FindAsync(id);

    if (project == null)
    {
        return Results.NotFound();
    }

    db.Projects.Remove(project);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
