namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;

public class LuminaUnlockSourceProvider : IUnlockSourceProvider {
    private IDataManager dataManager;
    private ILocalizationService localization;
    private Dictionary<uint, string> unlockCache;

    public LuminaUnlockSourceProvider(IDataManager dataManager, ILocalizationService localization) {
        this.dataManager = dataManager;
        this.localization = localization;
        this.unlockCache = new Dictionary<uint, string>();

        this.BuildCache();
    }

    private void BuildCache() {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        var itemSheet = this.dataManager.GetExcelSheet<Item>();
        var questSheet = this.dataManager.GetExcelSheet<Quest>();
        var achievementSheet = this.dataManager.GetExcelSheet<Achievement>();

        if (emoteSheet == null || itemSheet == null) return;

        var unlockLinkToEmote = new Dictionary<uint, uint>();
        foreach (var emote in emoteSheet) {
            if (emote.UnlockLink != 0) unlockLinkToEmote[emote.UnlockLink] = emote.RowId;
        }

        var itemToEmote = new Dictionary<uint, uint>();

        // Phase 1: Items & Mog Station Fallback
        foreach (var item in itemSheet) {
            if (!item.ItemAction.IsValid) continue;

            if (this.GetActionType(item.ItemAction.Value) != 2633) continue;

            var unlockId = item.ItemAction.Value.Data[0];
            if (unlockId != 0 && unlockLinkToEmote.TryGetValue(unlockId, out var emoteId)) {
                itemToEmote[item.RowId] = emoteId;

                var itemName = item.Name.ToString();
                if (string.IsNullOrEmpty(itemName)) continue;

                bool isUntradable = this.GetBoolProperty(item, "IsUntradable");
                uint price = this.GetUIntProperty(item, "PriceMid");

                // If an item is untradable and costs 0 Gil, it's highly likely from Mog Station or Seasonal Events
                if (isUntradable && price == 0) this.unlockCache[emoteId] = this.localization.Translate("src_mogstation", itemName);
                else this.unlockCache[emoteId] = this.localization.Translate("src_item", itemName);
            }
        }

        // Phase 2: Quests Cross-referencing
        if (questSheet != null) {
            var emoteRewardProp = typeof(Quest).GetProperty("EmoteReward") ?? typeof(Quest).GetProperty("ActionReward");
            var itemRewardProp = typeof(Quest).GetProperty("ItemReward");

            foreach (var quest in questSheet) {
                var questName = quest.Name.ToString();
                if (string.IsNullOrEmpty(questName)) continue;

                if (emoteRewardProp != null) {
                    var emoteId = this.ExtractId(emoteRewardProp.GetValue(quest));
                    if (emoteId != 0 && unlockLinkToEmote.ContainsValue(emoteId)) this.unlockCache[emoteId] = this.localization.Translate("src_quest", questName);
                }

                if (itemRewardProp != null) {
                    var itemRewards = itemRewardProp.GetValue(quest) as IEnumerable;
                    if (itemRewards != null) {
                        foreach (var reward in itemRewards) {
                            var itemId = this.ExtractId(reward);
                            // Override Item definition with the specific Quest definition
                            if (itemId != 0 && itemToEmote.TryGetValue(itemId, out var emoteId)) this.unlockCache[emoteId] = this.localization.Translate("src_quest", questName);
                        }
                    }
                }
            }
        }

        // Phase 3: Achievements Cross-referencing
        if (achievementSheet != null) {
            var itemProp = typeof(Achievement).GetProperty("Item");
            if (itemProp != null) {
                foreach (var achievement in achievementSheet) {
                    var itemId = this.ExtractId(itemProp.GetValue(achievement));
                    // Override Item definition with the specific Achievement definition
                    if (itemId != 0 && itemToEmote.TryGetValue(itemId, out var emoteId)) {
                        var achName = achievement.Name.ToString();
                        if (!string.IsNullOrEmpty(achName)) this.unlockCache[emoteId] = this.localization.Translate("src_achievement", achName);
                    }
                }
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

    private bool GetBoolProperty(object obj, string name) {
        var prop = obj.GetType().GetProperty(name);
        if (prop != null) return Convert.ToBoolean(prop.GetValue(obj));
        return false;
    }

    private uint GetUIntProperty(object obj, string name) {
        var prop = obj.GetType().GetProperty(name);
        if (prop != null) return Convert.ToUInt32(prop.GetValue(obj));
        return 0;
    }

    private uint ExtractId(object? obj) {
        if (obj == null) return 0;
        var type = obj.GetType();

        if (type.IsPrimitive) return Convert.ToUInt32(obj);

        var rowIdProp = type.GetProperty("RowId");
        if (rowIdProp != null) return Convert.ToUInt32(rowIdProp.GetValue(obj));

        var itemProp = type.GetProperty("Item") ?? type.GetProperty("ItemReward");
        if (itemProp != null) {
            var innerObj = itemProp.GetValue(obj);
            if (innerObj != null) {
                var innerRowId = innerObj.GetType().GetProperty("RowId");
                if (innerRowId != null) return Convert.ToUInt32(innerRowId.GetValue(innerObj));
                if (innerObj.GetType().IsPrimitive) return Convert.ToUInt32(innerObj);
            }
        }

        try {
            return Convert.ToUInt32(obj);
        }
        catch {
            return 0;
        }
    }

    public string GetUnlockSource(uint emoteId) {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) return this.localization.Translate("src_unknown");

        var emote = emoteSheet.GetRowOrDefault(emoteId);
        if (!emote.HasValue) return this.localization.Translate("src_unknown");

        if (emote.Value.UnlockLink == 0) return this.localization.Translate("src_default");

        if (this.unlockCache.TryGetValue(emoteId, out var source)) return source;

        return this.localization.Translate("src_unknown");
    }
}