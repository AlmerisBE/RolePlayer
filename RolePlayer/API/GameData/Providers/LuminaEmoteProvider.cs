namespace RolePlayer.API.GameData.Providers;

using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Collections.Generic;
using System.Linq;

public class LuminaEmoteProvider : IRawEmoteRepository {
    private IDataManager dataManager;
    private IUnlockSourceProvider unlockSourceProvider;
    private IClientState clientState;

    private readonly HashSet<uint> emotesWithVariations = new() { 50, 52, 53, 174 };

    public LuminaEmoteProvider(IDataManager dataManager, IUnlockSourceProvider unlockSourceProvider, IClientState clientState) {
        this.dataManager = dataManager;
        this.unlockSourceProvider = unlockSourceProvider;
        this.clientState = clientState;
    }

    public IEnumerable<EnrichedEmote> GetBaseEmotes() {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        var textCommandSheetEn = this.dataManager.GetExcelSheet<TextCommand>(ClientLanguage.English);

        if (emoteSheet == null) return Enumerable.Empty<EnrichedEmote>();

        return emoteSheet
            .Where(e => !string.IsNullOrEmpty(e.Name.ToString()) &&
                        e.Icon != 0 &&
                        e.TextCommand.IsValid &&
                        !string.IsNullOrWhiteSpace(e.TextCommand.Value.Command.ToString()))
            .Select(e => {
                var localizedCommand = e.TextCommand.Value.Command.ToString();
                var englishCommand = string.Empty;

                if (textCommandSheetEn != null) {
                    var enRow = textCommandSheetEn.GetRowOrDefault(e.TextCommand.RowId);
                    if (enRow.HasValue) englishCommand = enRow.Value.Command.ToString();
                }

                return new EnrichedEmote {
                    Id = e.RowId,
                    Name = e.Name.ToString(),
                    IconId = e.Icon,
                    IsUnlockable = e.UnlockLink != 0,
                    UnlockRequirement = this.unlockSourceProvider.GetUnlockSource(e.RowId),
                    Category = e.EmoteCategory.IsValid ? e.EmoteCategory.Value.Name.ToString() : string.Empty,
                    LocalizedCommand = localizedCommand,
                    EnglishCommand = englishCommand,
                    HasVariations = this.emotesWithVariations.Contains(e.RowId)
                };
            });
    }
}