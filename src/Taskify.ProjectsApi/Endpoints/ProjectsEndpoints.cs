using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Taskify.ProjectsApi.Data;

namespace Taskify.ProjectsApi.Endpoints;

public static class ProjectsEndpoints
{
    public static IEndpointRouteBuilder MapProjectsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/projects");

        group.MapGet("/", GetProjectsAsync);
        group.MapGet("/{id:guid}", GetProjectByIdAsync);
        group.MapGet("/{id:guid}/exists", GetProjectExistsAsync);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<ProjectResponse>>> GetProjectsAsync(
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var projects = await dbContext.Projects
            .AsNoTracking()
            .OrderBy(project => project.CreatedAtUtc)
            .Select(project => new ProjectResponse(project.Id, project.Name, project.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyList<ProjectResponse>>(projects);
    }

    private static async Task<Results<Ok<ProjectResponse>, NotFound>> GetProjectByIdAsync(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Where(candidate => candidate.Id == id)
            .Select(candidate => new ProjectResponse(candidate.Id, candidate.Name, candidate.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return project is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(project);
    }

    private static async Task<Ok<ProjectExistsResponse>> GetProjectExistsAsync(
        Guid id,
        ProjectsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Projects
            .AsNoTracking()
            .AnyAsync(project => project.Id == id, cancellationToken);

        return TypedResults.Ok(new ProjectExistsResponse(exists));
    }

    internal sealed record ProjectResponse(Guid Id, string Name, DateTime CreatedAtUtc);

    internal sealed record ProjectExistsResponse(bool Exists);
}
