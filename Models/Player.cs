namespace SRp.Models
{
    public sealed class Player
    {
        public int Id { get; set; }
        public long SteamId64 { get; set; }
        public long LastSeenAt { get; set; }
        public long CreatedAt { get; set; }
    }

    public sealed class Character
    {
        public int Id { get; set; }

        public long CharacterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }

        public int PlayerId { get; set; }
        public Player CharacterOwner { get; set; } = null!;
    }

    public sealed class CharacterDto
    {
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
