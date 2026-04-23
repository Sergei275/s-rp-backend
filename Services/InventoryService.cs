using SRp.Data;
using SRp.Models;
using Microsoft.EntityFrameworkCore;

namespace SRp.Services
{
    public class InventoryService
    {
        private readonly PlayerInventoryContext _inventoryContext;

        public InventoryService(PlayerInventoryContext db)
        {
            _inventoryContext = db; 
        }

        //public async Task AddItemsAsync(long ownerSteamId, IEnumerable<ItemDto> addedItems, CancellationToken ct = default)
        //{
        //    var list = addedItems.ToList();

        //    if (list.Any(x => x.Count < 0))
        //        throw new InvalidOperationException("AddItemsAsync: Count must be > 0");

        //    var merged = list
        //    .GroupBy(x => x.ItemId)
        //    .Select(g => new SavedStackItem { ItemId = g.Key, Count = g.Sum(x => x.Count) })
        //    .Where(x => x.Count != 0)
        //    .ToList();

        //    var playerInventory = await _inventoryContext.PlayerInventories
        //        .Include(x => x.StackItems)
        //        .FirstOrDefaultAsync(inventory => inventory.OwnerSteamId64 == ownerSteamId, ct);

        //    if (playerInventory == null)
        //        throw new InvalidOperationException("Player inventory not found");

        //    foreach (var item in merged)
        //    {
        //        var playerItem = playerInventory.StackItems
        //            .FirstOrDefault(list => list.ItemId == item.ItemId);

        //        if (playerItem == null)
        //        {
        //            item.OwnerSteamId64 = ownerSteamId;
        //            item.PlayerInventoryId = playerInventory.Id;
        //            item.PlayerInventory = playerInventory;

        //            playerInventory.StackItems.Add(item);
        //        }

        //        if (playerItem != null)
        //            playerItem.Count += item.Count;
        //    }

        //    await _inventoryContext.SaveChangesAsync(ct);
        //}

        //public async Task RemoveItemsAsync(long ownerSteamId, IEnumerable<ItemDto> removedItems, CancellationToken ct = default)
        //{
        //    var list = removedItems.ToList();

        //    if (list.Any(x => x.Count < 0))
        //        throw new InvalidOperationException("RemoveItemsAsync: Count must be > 0");

        //    var merged = list
        //        .GroupBy(x => x.ItemId)
        //        .Select(g => new SavedStackItem { ItemId = g.Key, Count = g.Sum(x => x.Count) })
        //        .Where(x => x.Count != 0)
        //        .ToList();

        //    var playerInventory = await _inventoryContext.PlayerInventories
        //        .Include(x => x.StackItems)
        //        .FirstOrDefaultAsync(inventory => inventory.OwnerSteamId64 == ownerSteamId, ct);

        //    if (playerInventory == null)
        //        throw new InvalidOperationException("Player inventory not found");

        //    foreach (var item in merged)
        //    {
        //        var playerItem = playerInventory.StackItems
        //            .FirstOrDefault(list => list.ItemId == item.ItemId);

        //        if (playerItem == null)
        //            throw new InvalidOperationException($"'{item.ItemId}' not found");

        //        if (playerItem.Count < item.Count)
        //            throw new InvalidOperationException($"Couldn't delete '{item.ItemId}' because it's too small");

        //        playerItem.Count -= item.Count;

        //        if (playerItem.Count == 0)
        //            _inventoryContext.Remove(playerItem);
        //    }

        //    await _inventoryContext.SaveChangesAsync(ct);
        //}
    }
}
