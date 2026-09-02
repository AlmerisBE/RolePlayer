namespace RolePlayer.API.GameData.Providers;

using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;
using System.Linq;

public class LuminaEmoteProvider : IEmoteRepository {
    private IDataManager dataManager;
    private IUnlockSourceProvider unlockSourceProvider;
    private IClientState clientState;

    public LuminaEmoteProvider(IDataManager dataManager, IUnlockSourceProvider unlockSourceProvider, IClientState clientState) {
        this.dataManager = dataManager;
        this.unlockSourceProvider = unlockSourceProvider;
        this.clientState = clientState;
    }

    public IEnumerable<EmoteDisplayData> GetBaseEmotes() {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        var textCommandSheetEn = this.dataManager.GetExcelSheet<TextCommand>(ClientLanguage.English);

        if (emoteSheet == null) {
            return Enumerable.Empty<EmoteDisplayData>();
        }

        return emoteSheet
            .Where(e => !string.IsNullOrEmpty(e.Name.ToString()))
            .Select(e => {
                var localizedCommand = e.TextCommand.IsValid ? e.TextCommand.Value.Command.ToString() : string.Empty;
                var englishCommand = string.Empty;

                if (e.TextCommand.IsValid && textCommandSheetEn != null) {
                    var enRow = textCommandSheetEn.GetRowOrDefault(e.TextCommand.RowId);
                    if (enRow.HasValue) {
                        englishCommand = enRow.Value.Command.ToString();
                    }
                }

                return new EmoteDisplayData {
                    Id = e.RowId,
                    Name = e.Name.ToString(),
                    IconId = e.Icon,
                    IsUnlockable = e.UnlockLink != 0,
                    UnlockRequirement = this.unlockSourceProvider.GetUnlockSource(e.RowId),
                    Category = e.EmoteCategory.IsValid ? e.EmoteCategory.Value.Name.ToString() : string.Empty,
                    LocalizedCommand = localizedCommand,
                    EnglishCommand = englishCommand
                };
            });
    }
}