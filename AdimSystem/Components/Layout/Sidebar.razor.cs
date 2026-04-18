using System.Threading;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Navigations;

namespace AdimSystem.Components.Layout;

public partial class Sidebar
{
    private const string DefaultTarget = "#main-layout-content";

    [Parameter]
    public string MainContentTarget { get; set; } = DefaultTarget;

    private bool IsSidebarOpen { get; set; }

    /// <summary>When true, sidebar was opened with the toggle and stays open until toggled or viewport forces close.</summary>
    private bool _pinnedOpenByToggle;

    private bool _narrowViewport;

    private CancellationTokenSource? _collapseCts;

    private IJSObjectReference? _jsModule;

    private IJSObjectReference? _viewportSubscription;

    private DotNetObjectReference<Sidebar>? _dotNetRef;

    private SidebarType SidebarDisplayType => _narrowViewport ? SidebarType.Over : SidebarType.Push;

    [JSInvokable]
    public Task OnNarrowViewport(bool isNarrow)
    {
        return InvokeAsync(() =>
        {
            _narrowViewport = isNarrow;
            if (isNarrow)
            {
                _collapseCts?.Cancel();
                _collapseCts?.Dispose();
                _collapseCts = null;
                IsSidebarOpen = false;
                _pinnedOpenByToggle = false;
            }

            StateHasChanged();
        });
    }

    private void OnToggleClick()
    {
        IsSidebarOpen = !IsSidebarOpen;
        _pinnedOpenByToggle = IsSidebarOpen;
        _collapseCts?.Cancel();
        _collapseCts?.Dispose();
        _collapseCts = null;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _dotNetRef = DotNetObjectReference.Create(this);
        _jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/sidebar-layout.js");
        _viewportSubscription = await _jsModule.InvokeAsync<IJSObjectReference>(
            "subscribeNarrowViewport",
            _dotNetRef,
            SidebarOptions.NarrowViewportMaxWidthPixels);
    }

    private void OnNavScrollMouseEnter()
    {
        if (_narrowViewport)
        {
            return;
        }

        _collapseCts?.Cancel();
        _collapseCts?.Dispose();
        _collapseCts = null;

        if (!IsSidebarOpen)
        {
            IsSidebarOpen = true;
            StateHasChanged();
        }
    }

    private async Task OnNavScrollMouseLeaveAsync()
    {
        if (_narrowViewport || _pinnedOpenByToggle)
        {
            return;
        }

        _collapseCts?.Cancel();
        _collapseCts?.Dispose();
        _collapseCts = new CancellationTokenSource();
        var token = _collapseCts.Token;

        try
        {
            await Task.Delay(SidebarOptions.CollapseDelayMilliseconds, token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        IsSidebarOpen = false;
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        _collapseCts?.Cancel();
        _collapseCts?.Dispose();
        _collapseCts = null;

        if (_viewportSubscription is not null)
        {
            try
            {
                await _viewportSubscription.InvokeVoidAsync("dispose");
            }
            catch (JSDisconnectedException)
            {
                // Circuit gone
            }

            await _viewportSubscription.DisposeAsync();
        }

        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }

        _dotNetRef?.Dispose();
    }
}
