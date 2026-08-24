using Taskify.ServiceDefaults;
using Taskify.Web.Components;
using Taskify.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ProjectsApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://projectsapi");
});

builder.Services.AddHttpClient<TasksApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://tasksapi");
});

builder.Services.AddHttpClient<NotificationsApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://notificationsapi");
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
