using Microsoft.EntityFrameworkCore;
using Taskify.NotificationsApi.Data;
using Taskify.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddDbContext<NotificationsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("NotificationsDb")
        ?? builder.Configuration["ConnectionStrings:NotificationsDb"]
        ?? "Data Source=notifications.db";

    options.UseSqlite(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await NotificationsDbInitializer.InitializeAsync(app.Services);

app.MapGet("/", () => Results.Ok(new { service = "Taskify.NotificationsApi" }));
app.MapHub<Taskify.NotificationsApi.Hubs.BoardHub>("/hubs/board");
app.MapDefaultEndpoints();

app.Run();
