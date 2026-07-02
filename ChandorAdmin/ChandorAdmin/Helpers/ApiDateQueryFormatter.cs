using System.Globalization;

namespace ChandorAdmin.Helpers;

/// <summary>
/// Culture-invariant formatters for date/time values sent as API query parameters.
/// </summary>
public static class ApiDateQueryFormatter
{
    /// <summary>
    /// Formats a date/time using the ISO 8601 round-trip format expected by Chandor API endpoints.
    /// </summary>
    public static string Format(DateTime value, bool asUtc = false)
    {
        var dateTime = asUtc ? value.ToUniversalTime() : value;
        return dateTime.ToString("o", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a nullable date/time using the ISO 8601 round-trip format expected by Chandor API endpoints.
    /// </summary>
    public static string Format(DateTime? value, bool asUtc = false)
        => value is { } dateTime ? Format(dateTime, asUtc) : string.Empty;

    /// <summary>
    /// Formats and URL-encodes a date/time for use as a query-string value.
    /// </summary>
    public static string FormatQueryValue(DateTime value, bool asUtc = false)
        => Uri.EscapeDataString(Format(value, asUtc));

    /// <summary>
    /// Formats and URL-encodes a nullable date/time for use as a query-string value.
    /// </summary>
    public static string FormatQueryValue(DateTime? value, bool asUtc = false)
        => value is { } dateTime ? FormatQueryValue(dateTime, asUtc) : string.Empty;
}
