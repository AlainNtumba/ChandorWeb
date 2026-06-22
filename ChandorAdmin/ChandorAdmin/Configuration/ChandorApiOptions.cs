namespace ChandorAdmin.Configuration;

/// <summary>
/// Base URL of Chandor API (typically ends with <c>/api/</c>) and the URL segment for API versioning (e.g. <c>1.0</c> → .../v1.0/...).
/// </summary>
public sealed class ChandorApiOptions
{
    public const string SectionName = "ChandorApi";

    /// <summary>
    /// Root URL including the <c>/api/</c> segment (e.g. https://chandor.somee.com/api/ https://localhost:7145/api/).
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:7145/api/";

    /// <summary>
    /// Version segment for <c>v{segment}/</c> after the base URL (matches routes such as <c>/api/v1.0/...</c>).
    /// </summary>
    public string VersionPathSegment { get; set; } = "1.0";

    /// <summary>Relative path prefix for versioned controllers (e.g. <c>v1.0/</c>).</summary>
    public string VersionedRoot => $"v{VersionPathSegment.Trim('/')}/";
}
