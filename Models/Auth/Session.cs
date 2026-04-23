namespace SRp.Models
{
    public sealed class Session
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public long OwnerSteamId64 { get; set; }
        public long CreatedAt { get; set; }
        public long LastSeenAt { get; set; }
        public long ExpiresAt { get; set; }
        public long? RevokedAt { get; set; } = null;
    }
}
