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


app.Run();
