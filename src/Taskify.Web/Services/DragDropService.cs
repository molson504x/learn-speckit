using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Taskify.Web.Services;

public class DragDropService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private IJSObjectReference? module;

    public virtual async Task InitializeAsync<TComponent>(
        ElementReference boardElement,
        DotNetObjectReference<TComponent> callbackReference,
        CancellationToken cancellationToken = default)
        where TComponent : class
    {
        module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, "./js/sortable-interop.js");
        await module.InvokeVoidAsync("initializeKanbanBoard", cancellationToken, boardElement, callbackReference);
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }
}
