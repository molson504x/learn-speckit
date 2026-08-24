namespace Taskify.Web.Services;

public sealed class NotificationsApiClient(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;
}
