using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Taskify.ServiceDefaults;
using Taskify.Web.Components;
using Taskify.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddProtectedBrowserStorage();

builder.Services.AddHttpClient<ProjectsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProjectsApi:BaseUrl"] ?? "https://localhost:5001");
});

builder.Services.AddHttpClient<TasksApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TasksApi:BaseUrl"] ?? "https://localhost:5002");
});

builder.Services.AddHttpClient<NotificationsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationsApi:BaseUrl"] ?? "https://localhost:5003");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
