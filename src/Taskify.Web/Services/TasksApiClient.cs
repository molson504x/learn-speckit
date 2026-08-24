namespace Taskify.Web.Services;

public sealed class TasksApiClient(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;
}
