namespace SRp.Models
{
    public sealed class RefreshToken
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public string TokenHash { get; set; } = "";
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public int? ReplacedById { get; set; }
    }
}
