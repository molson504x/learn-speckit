using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Taskify.ServiceDefaults.ReferenceData;

namespace Taskify.ProjectsApi.Tests;

public sealed class ProjectsApiTests
{
    [Fact]
    public async Task GetProjects_ReturnsSeededProjectsWithContractShape()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/projects");
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectContract>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(projects);
        Assert.Equal(3, projects.Count);
        Assert.Equal(
            SampleProjects.All.Select(project => project.Name),
            projects.Select(project => project.Name));
        Assert.All(projects, project =>
        {
            Assert.NotEqual(Guid.Empty, project.Id);
            Assert.NotEqual(default, project.CreatedAtUtc);
        });
    }

    [Fact]
    public async Task GetProjectById_ReturnsProject()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = factory.CreateClient();

        var project = await client.GetFromJsonAsync<ProjectContract>($"/api/projects/{SampleProjects.BugTriageId}");

        Assert.NotNull(project);
        Assert.Equal(SampleProjects.BugTriageId, project.Id);
        Assert.Equal("Bug Triage", project.Name);
    }

    private sealed record ProjectContract(Guid Id, string Name, DateTime CreatedAtUtc);

    private sealed class ProjectsApiFactory : WebApplicationFactory<global::Program>
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"taskify-projects-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ProjectsDb"] = $"Data Source={databasePath}",
                });
            });
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
