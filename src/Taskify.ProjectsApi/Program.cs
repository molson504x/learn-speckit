using Microsoft.EntityFrameworkCore;
using Taskify.ProjectsApi.Data;
using Taskify.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProjectsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ProjectsDb")
        ?? builder.Configuration["ConnectionStrings:ProjectsDb"]
        ?? "Data Source=projects.db";

    options.UseSqlite(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await ProjectsSeeder.InitializeAsync(app.Services);

app.MapGet("/", () => Results.Ok(new { service = "Taskify.ProjectsApi" }));
app.MapDefaultEndpoints();

app.Run();
