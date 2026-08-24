using Microsoft.EntityFrameworkCore;
using Taskify.ServiceDefaults;
using Taskify.TasksApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<TasksDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("TasksDb")
        ?? builder.Configuration["ConnectionStrings:TasksDb"]
        ?? "Data Source=tasks.db";

    options.UseSqlite(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await TasksDbInitializer.InitializeAsync(app.Services);

app.MapGet("/", () => Results.Ok(new { service = "Taskify.TasksApi" }));
app.MapDefaultEndpoints();

app.Run();
