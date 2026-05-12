using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChandorAdmin.Services.Auth;

/// <summary>Resolves display name, initials, and optional photo URL from access-token claims.</summary>
public static class UserProfileClaimsMapper
{
    public sealed record UserDisplayProfile(string DisplayName, string Initials, string? PhotoUrl);

    public static UserDisplayProfile FromClaimsPrincipal(ClaimsPrincipal? user)
    {
        if (user?.Identity is not { IsAuthenticated: true })
            return new UserDisplayProfile("Signed-in user", "?", null);

        var claims = user.Claims.ToList();
        var given = FirstNonEmpty(claims, ClaimTypes.GivenName, JwtRegisteredClaimNames.GivenName, "given_name", "Name");
        var family = FirstNonEmpty(claims, ClaimTypes.Surname, JwtRegisteredClaimNames.FamilyName, "family_name", "Surname");

        string display;
        if (!string.IsNullOrWhiteSpace(given) && !string.IsNullOrWhiteSpace(family))
            display = $"{given.Trim()} {family.Trim()}".Trim();
        else if (!string.IsNullOrWhiteSpace(given))
            display = given.Trim();
        else if (!string.IsNullOrWhiteSpace(family))
            display = family.Trim();
        else
        {
            display = FirstNonEmpty(claims, ClaimTypes.Name, JwtRegisteredClaimNames.Name, "name", "unique_name", JwtRegisteredClaimNames.PreferredUsername, ClaimTypes.Email)
                      ?? "Signed-in user";
        }

        var photo = FirstNonEmpty(claims, "picture", "profile_photo", "avatar_url", "ProfileLink");

        var initials = BuildInitials(given, family, display);
        return new UserDisplayProfile(display.Trim(), initials, photo);
    }

    private static string? FirstNonEmpty(IEnumerable<Claim> claims, params string[] types)
    {
        foreach (var type in types)
        {
            var v = claims.FirstOrDefault(c => string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase))?.Value;
            if (!string.IsNullOrWhiteSpace(v))
                return v.Trim();
        }

        return null;
    }

    private static string BuildInitials(string? given, string? family, string display)
    {
        if (!string.IsNullOrWhiteSpace(given) && !string.IsNullOrWhiteSpace(family))
            return $"{char.ToUpper(given![0], CultureInfo.CurrentCulture)}{char.ToUpper(family![0], CultureInfo.CurrentCulture)}";

        var parts = display.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
            return $"{char.ToUpper(parts[0][0], CultureInfo.CurrentCulture)}{char.ToUpper(parts[1][0], CultureInfo.CurrentCulture)}";

        if (parts.Length == 1 && parts[0].Length >= 2)
            return parts[0].Substring(0, 2).ToUpperInvariant();

        if (parts.Length == 1 && parts[0].Length == 1)
            return parts[0].ToUpperInvariant();

        return "?";
    }
}
