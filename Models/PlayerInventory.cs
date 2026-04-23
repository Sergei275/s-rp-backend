namespace SRp.Models
{
    public sealed class PlayerInventory
    {
        public int Id { get; set; }
        public long OwnerSteamId64 { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

       // public List<SavedStackItem> StackItems { get; set; } = new();
    }
}
