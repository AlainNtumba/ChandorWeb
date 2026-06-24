using Microsoft.AspNetCore.Components;

namespace ChandorAdmin.Services;

/// <summary>Navigation helpers that respect the Blazor app base path (GitHub Pages subfolder).</summary>
public sealed class AppNavigation
{
    private readonly NavigationManager _navigation;

    public AppNavigation(NavigationManager navigation) => _navigation = navigation;

    public void NavigateToLogin(bool replace = false) => NavigateToRelative("login", replace);

    public void NavigateToHome(bool replace = false) => NavigateToRelative(string.Empty, replace);

    private void NavigateToRelative(string path, bool replace)
    {
        _ = replace;
        var target = string.IsNullOrEmpty(path)
            ? _navigation.BaseUri
            : $"{_navigation.BaseUri.TrimEnd('/')}/{path.TrimStart('/')}";

        var relativePath = _navigation.ToBaseRelativePath(target);
        _navigation.NavigateTo(relativePath, forceLoad: false);
    }
}
