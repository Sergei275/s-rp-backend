using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int AccessMinutes { get; set; } = 60;
}

public sealed class RefreshOptions
{
    public int Days { get; set; } = 14;
}

public sealed class TokenService
{
    private readonly JwtOptions _jwt;
    private readonly RefreshOptions _refresh;

    public TokenService(IOptions<JwtOptions> jwt, IOptions<RefreshOptions> refresh)
    {
        _jwt = jwt.Value;
        _refresh = refresh.Value;
    }

    public (string token, long expiresInSeconds) CreateAccessToken(long steamId64, Guid sessionId)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, steamId64.ToString()),
            new("sid", sessionId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_jwt.AccessMinutes),
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        var expiresIn = (long)TimeSpan.FromMinutes(_jwt.AccessMinutes).TotalSeconds;
        return (token, expiresIn);
    }

    public (string refreshToken, DateTime expiresAtUtc) CreateRefreshToken()
    {
        // 32 байта крипто-рандома
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes); // можно заменить на Base64Url при желании
        var expires = DateTime.UtcNow.AddDays(_refresh.Days);
        return (token, expires);
    }

    public static string Sha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash); // удобный строковый вид
    }
}