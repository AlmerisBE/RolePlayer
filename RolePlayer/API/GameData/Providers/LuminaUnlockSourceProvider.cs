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
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        var itemSheet = this.dataManager.GetExcelSheet<Item>();

        if (emoteSheet == null || itemSheet == null) {
            return;
        }

        var emoteUnlockLinks = new HashSet<uint>();
        foreach (var emote in emoteSheet) {
            if (emote.UnlockLink != 0) {
                emoteUnlockLinks.Add(emote.UnlockLink);
            }
        }

        foreach (var item in itemSheet) {
            var itemAction = item.ItemAction.ValueNullable;
            if (!itemAction.HasValue) {
                continue;
            }

            var unlockId = itemAction.Value.Data[0];

            if (unlockId != 0 && emoteUnlockLinks.Contains(unlockId)) {
                // ItemUICategory 63 = Miscellany (Catégorie des manuels en jeu)
                if (item.ItemUICategory.RowId == 63) {
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

        if (emote.Value.UnlockLink == 0) {
            return "Default Emote / No Unlock Required";
        }

        var unlockLink = emote.Value.UnlockLink;
        if (this.itemUnlockCache.TryGetValue(unlockLink, out var source)) {
            return source;
        }

        return "Quest, Achievement, or Mog Station";
    }
}