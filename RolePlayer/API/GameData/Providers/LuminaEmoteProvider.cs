namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;
using System.Linq;

public class LuminaEmoteProvider : IEmoteRepository {
    private IDataManager dataManager;
    private IUnlockSourceProvider unlockSourceProvider;

    public LuminaEmoteProvider(IDataManager dataManager, IUnlockSourceProvider unlockSourceProvider) {
        this.dataManager = dataManager;
        this.unlockSourceProvider = unlockSourceProvider;
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
                UnlockRequirement = this.unlockSourceProvider.GetUnlockSource(e.RowId),
                Category = e.EmoteCategory.IsValid ? e.EmoteCategory.Value.Name.ToString() : string.Empty
            });
    }
}