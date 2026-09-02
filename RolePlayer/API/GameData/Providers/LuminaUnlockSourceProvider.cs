namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Collections.Generic;

public class LuminaUnlockSourceProvider : IUnlockSourceProvider {
    private IDataManager dataManager;
    private Dictionary<uint, string> itemUnlockCache;

    public LuminaUnlockSourceProvider(IDataManager dataManager) {
        this.dataManager = dataManager;
        this.itemUnlockCache = new Dictionary<uint, string>();

        this.BuildCache();
    }

    private void BuildCache() {
        var itemSheet = this.dataManager.GetExcelSheet<Item>();
        if (itemSheet == null) {
            return;
        }

        // FFXIV architecture: ItemAction type 26 is used to unlock Mounts, Minions, Hairstyles, and Emotes.
        // Data[0] contains the UnlockLink ID which correlates to Emote.UnlockLink.
        foreach (var item in itemSheet) {
            var itemAction = item.ItemAction.ValueNullable;
            if (!itemAction.HasValue) {
                continue;
            }

            if (itemAction.Value.Type == 26) {
                var unlockId = itemAction.Value.Data[0];
                if (unlockId != 0 && !this.itemUnlockCache.ContainsKey(unlockId)) {
                    var itemName = item.Name.ToString();
                    if (!string.IsNullOrEmpty(itemName)) {
                        this.itemUnlockCache[unlockId] = $"Item: {itemName}";
                    }
                }
            }
        }
    }

    public string GetUnlockSource(uint emoteId) {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return "Unknown";
        }

        var emote = emoteSheet.GetRowOrDefault(emoteId);
        if (!emote.HasValue) {
            return "Unknown";
        }

        // If the emote has no unlock requirement, it is available from the start
        if (emote.Value.UnlockLink == 0) {
            return "Default Emote / No Unlock Required";
        }

        // Check if our cache found an item that triggers this UnlockLink
        var unlockLink = emote.Value.UnlockLink;
        if (this.itemUnlockCache.TryGetValue(unlockLink, out var source)) {
            return source;
        }

        // If no item is found, it's typically tied to a Quest, Achievement, or Mog Station purchase
        return "Quest, Achievement, or Mog Station";
    }
}