namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;
using System.Linq;

public class LuminaEmoteProvider : IEmoteRepository {
    private IDataManager dataManager;

    public LuminaEmoteProvider(IDataManager dataManager) {
        this.dataManager = dataManager;
    }

    public IEnumerable<EmoteDisplayData> GetBaseEmotes() {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return Enumerable.Empty<EmoteDisplayData>();
        }

        return emoteSheet
            .Where(e => !string.IsNullOrEmpty(e.Name.ToString()))
            .Select(e => new EmoteDisplayData {
                Id = e.RowId,
                Name = e.Name.ToString(),
                IconId = e.Icon,
                IsUnlockable = e.UnlockLink != 0,
                UnlockRequirement = string.Empty, // Will be implemented later with another Excel sheet
                Category = e.EmoteCategory.IsValid ? e.EmoteCategory.Value.Name.ToString() : string.Empty
            });
    }
}