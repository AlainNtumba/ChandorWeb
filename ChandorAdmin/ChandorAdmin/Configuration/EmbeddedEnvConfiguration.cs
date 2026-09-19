using System.Reflection;

namespace ChandorAdmin.Configuration;

/// <summary>
/// Loads the repository-level .env file embedded at build time. Values loaded here
/// override appsettings.json. Do not use this mechanism for secrets because a
/// Blazor WebAssembly application is downloaded by the browser.
/// </summary>
internal static class EmbeddedEnvConfiguration
{
    private const string ResourceName = "ChandorAdmin.GlobalEnv";

    private static readonly IReadOnlyDictionary<string, string> KeyMappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CHANDOR_API_BASE_URL"] = "ChandorApi:BaseUrl",
            ["CHANDOR_API_VERSION_PATH_SEGMENT"] = "ChandorApi:VersionPathSegment"
        };

    public static IReadOnlyDictionary<string, string?> Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
        if (stream is null)
            return new Dictionary<string, string?>();

        using var reader = new StreamReader(stream);
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
                continue;

            var envKey = trimmed[..separator].Trim();
            if (!KeyMappings.TryGetValue(envKey, out var configurationKey))
                continue;

            var value = Unquote(trimmed[(separator + 1)..].Trim());
            if (value.Length > 0)
                values[configurationKey] = value;
        }

        return values;
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2
            && ((value[0] == '"' && value[^1] == '"')
                || (value[0] == '\'' && value[^1] == '\'')))
        {
            return value[1..^1];
        }

        return value;
    }
}
