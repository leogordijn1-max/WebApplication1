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

app.MapGet("/projects", async (AppDbContext db) =>
{
    var projecten = await db.Projects.ToListAsync();

    var dtos = projecten.Select(project => new ProjectDto
    {
        Id = project.Id,
        Titel = project.Titel,
        Beschrijving = project.Beschrijving,
        Categorie = project.Categorie,
        GitHubUrl = project.GitHubUrl,
        Datum = project.Datum
    }).ToList();

    return Results.Ok(dtos);
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

    var resultaat = new ProjectDto
    {
        Id = project.Id,
        Titel = project.Titel,
        Beschrijving = project.Beschrijving,
        Categorie = project.Categorie,
        GitHubUrl = project.GitHubUrl,
        Datum = project.Datum
    };

    return Results.Created($"/projects/{project.Id}", resultaat);
});

// PUT //

app.MapPut("/projects/{id}", async (int id, UpdateProjectDto dto, AppDbContext db) =>
{
    var project = await db.Projects.FindAsync(id);

    if (project == null)
    {
        return Results.NotFound();
    }

    project.Titel = dto.Titel;
    project.Beschrijving = dto.Beschrijving;
    project.Categorie = dto.Categorie;
    project.GitHubUrl = dto.GitHubUrl;
    project.Datum = dto.Datum;

    await db.SaveChangesAsync();

    return Results.Ok(new ProjectDto
    {
        Id = project.Id,
        Titel = project.Titel,
        Beschrijving = project.Beschrijving,
        Categorie = project.Categorie,
        GitHubUrl = project.GitHubUrl,
        Datum = project.Datum
    });
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

    var resultaat = new ProjectDto
{
    Id = project.Id,
    Titel = project.Titel,
    Beschrijving = project.Beschrijving,
    Categorie = project.Categorie,
    GitHubUrl = project.GitHubUrl,
    Datum = project.Datum
};

return Results.Created($"/projects/{project.Id}", resultaat);
});


// BLOGPOSTS - GET ALL
app.MapGet("/blogposts", async (AppDbContext db) =>
{
    var blogposts = await db.Blogposts.ToListAsync();

    var dtos = blogposts.Select(blogpost => new BlogpostDto
    {
        Id = blogpost.Id,
        Titel = blogpost.Titel,
        Inhoud = blogpost.Inhoud,
        Publicatiedatum = blogpost.Publicatiedatum
    }).ToList();

    return Results.Ok(dtos);
});

// BLOGPOSTS - GET BY ID
app.MapGet("/blogposts/{id}", async (int id, AppDbContext db) =>
{
    var blogpost = await db.Blogposts.FindAsync(id);

    if (blogpost == null)
        return Results.NotFound();

    return Results.Ok(new BlogpostDto
    {
        Id = blogpost.Id,
        Titel = blogpost.Titel,
        Inhoud = blogpost.Inhoud,
        Publicatiedatum = blogpost.Publicatiedatum
    });
});

// BLOGPOSTS - POST
app.MapPost("/blogposts", async (CreateBlogpostDto dto, AppDbContext db) =>
{
    var blogpost = new Blogpost
    {
        Titel = dto.Titel,
        Inhoud = dto.Inhoud,
        Publicatiedatum = dto.Publicatiedatum
    };

    db.Blogposts.Add(blogpost);
    await db.SaveChangesAsync();

    var resultaat = new BlogpostDto
    {
        Id = blogpost.Id,
        Titel = blogpost.Titel,
        Inhoud = blogpost.Inhoud,
        Publicatiedatum = blogpost.Publicatiedatum
    };

    return Results.Created($"/blogposts/{blogpost.Id}", resultaat);
});

// BLOGPOSTS - PUT
app.MapPut("/blogposts/{id}", async (int id, UpdateBlogpostDto dto, AppDbContext db) =>
{
    var blogpost = await db.Blogposts.FindAsync(id);

    if (blogpost == null)
        return Results.NotFound();

    blogpost.Titel = dto.Titel;
    blogpost.Inhoud = dto.Inhoud;
    blogpost.Publicatiedatum = dto.Publicatiedatum;

    await db.SaveChangesAsync();

    return Results.Ok(new BlogpostDto
    {
        Id = blogpost.Id,
        Titel = blogpost.Titel,
        Inhoud = blogpost.Inhoud,
        Publicatiedatum = blogpost.Publicatiedatum
    });
});

// BLOGPOSTS - DELETE
app.MapDelete("/blogposts/{id}", async (int id, AppDbContext db) =>
{
    var blogpost = await db.Blogposts.FindAsync(id);

    if (blogpost == null)
        return Results.NotFound();

    db.Blogposts.Remove(blogpost);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.Run();
