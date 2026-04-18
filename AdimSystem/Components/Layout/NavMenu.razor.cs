using Microsoft.AspNetCore.Components;

namespace AdimSystem.Components.Layout;

public partial class NavMenu
{
    [Parameter]
    public bool IsExpanded { get; set; }

    private string? GetItemTitle(string text) => IsExpanded ? null : text;
}
