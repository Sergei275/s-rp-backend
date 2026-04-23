namespace SRp.Models
{
    public sealed class AuthTokensResponse
    {
        public string AccessToken { get; set; } = "";
        public long ExpiresInSeconds { get; set; }
        public string RefreshToken { get; set; } = "";
    }
}
