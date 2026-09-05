namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
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

        if (emoteSheet == null || itemSheet == null) return;

        var emoteUnlockLinks = new HashSet<uint>();
        foreach (var emote in emoteSheet) {
            if (emote.UnlockLink != 0) emoteUnlockLinks.Add(emote.UnlockLink);
        }

        foreach (var item in itemSheet) {
            if (!item.ItemAction.IsValid) continue;

            var itemAction = item.ItemAction.Value;
            ushort actionType = this.GetActionType(itemAction);

            // 2633 is the strictly designated ItemAction Type for unlocking Emotes.
            // This prevents ID collisions with Minions, Mounts, or Bardings stored in the same Data[0] index.
            if (actionType != 2633) continue;

            var unlockId = itemAction.Data[0];

            if (unlockId != 0 && emoteUnlockLinks.Contains(unlockId)) {
                var itemName = item.Name.ToString();
                if (!string.IsNullOrEmpty(itemName)) this.itemUnlockCache[unlockId] = itemName;
            }
        }
    }

    private ushort GetActionType(object actionObj) {
        if (actionObj == null) return 0;

        var type = actionObj.GetType();

        var prop = type.GetProperty("Type") ?? type.GetProperty("ActionType") ?? type.GetProperty("Unknown0");
        if (prop != null) return Convert.ToUInt16(prop.GetValue(actionObj));

        var field = type.GetField("Type") ?? type.GetField("ActionType") ?? type.GetField("Unknown0");
        if (field != null) return Convert.ToUInt16(field.GetValue(actionObj));

        return 0;
    }

    public string GetUnlockSource(uint emoteId) {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) return "Unknown";

        var emote = emoteSheet.GetRowOrDefault(emoteId);
        if (!emote.HasValue) return "Unknown";

        if (emote.Value.UnlockLink == 0) return "Default Emote / No Unlock Required";

        if (this.itemUnlockCache.TryGetValue(emote.Value.UnlockLink, out var source)) return source;

        return "Quest, Achievement, or Mog Station";
    }
}