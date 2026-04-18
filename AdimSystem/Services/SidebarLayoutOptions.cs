namespace AdimSystem.Services;

/// <summary>Configurable values for the app shell sidebar (registered for DI).</summary>
public sealed class SidebarLayoutOptions
{
    public int CollapseDelayMilliseconds { get; set; } = 175;

    public int NarrowViewportMaxWidthPixels { get; set; } = 768;
}
