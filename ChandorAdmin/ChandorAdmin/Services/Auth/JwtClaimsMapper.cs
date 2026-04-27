using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChandorAdmin.Services.Auth;

public static class JwtClaimsMapper
{
    /// <summary>Builds a <see cref="ClaimsPrincipal"/> from a JWT, preserving all original claims. Adds <see cref="ClaimTypes.NameIdentifier"/> from <c>sub</c> when available.</summary>
    public static ClaimsPrincipal? CreatePrincipalFromAccessToken(string? accessToken, string authenticationType = "Bearer")
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return null;

        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        }
        catch
        {
            return null;
        }

        var claims = new List<Claim>();
        foreach (var c in jwt.Claims)
        {
            claims.Add(c);
            if (c.Type is JwtRegisteredClaimNames.Sub)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, c.Value, c.ValueType, c.Issuer, c.OriginalIssuer));
        }

        var id = new ClaimsIdentity(claims, authenticationType, ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(id);
    }
}
