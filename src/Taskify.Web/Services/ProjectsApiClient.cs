namespace Taskify.Web.Services;

public sealed class ProjectsApiClient(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;
}
