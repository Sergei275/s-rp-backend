namespace SRp.Models
{
    public sealed class LoginRequest
    {
        public long SteamId64 { get; set; }
        public string FacepunchToken { get; set; } = string.Empty;
    }
}
