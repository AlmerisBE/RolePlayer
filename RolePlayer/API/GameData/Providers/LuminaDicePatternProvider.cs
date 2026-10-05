namespace RolePlayer.API.GameData.Providers;

using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.API.GameEvents.Contracts;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class LuminaDicePatternProvider : IDicePatternProvider {
    private IDataManager dataManager;
    private ILoggerService logger;

    public LuminaDicePatternProvider(IDataManager dataManager, ILoggerService logger) {
        this.dataManager = dataManager;
        this.logger = logger;
    }

    public IReadOnlyList<string> GetLocalizedDiceKeywords() {
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var enSheet = this.dataManager.GetExcelSheet<LogMessage>(ClientLanguage.English);
        var localSheet = this.dataManager.GetExcelSheet<LogMessage>();

        if (localSheet == null) return new List<string>();

        var diceIds = new HashSet<uint> { 856, 1231, 3887, 5180 };

        if (enSheet != null) {
            foreach (var row in enSheet) {
                string rawEn = row.Text.ExtractText().ToLowerInvariant();

                // English FFXIV dice rolls explicitly contain "random!" or "dice!" followed by "roll"
                if ((rawEn.Contains("random!") || rawEn.Contains("dice!")) && rawEn.Contains("roll")) {
                    diceIds.Add(row.RowId);
                }
            }
        }

        foreach (var id in diceIds) {
            var row = localSheet.GetRowOrDefault(id);
            if (!row.HasValue) continue;

            // ExtractText() strips all dynamic payloads natively, leaving pure text templates
            string cleanLocal = row.Value.Text.ExtractText();
            var parts = cleanLocal.Split(new[] { '!', '.', '(', ')', ':' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts) {
                string trimmed = part.Trim();

                // Keep only substantial phrase fragments to prevent generic false positives
                if (trimmed.Length > 5) {
                    this.logger.Info($"[LuminaDicePattern] Extracted secure pattern from ID {id}: '{trimmed}'");
                    keywords.Add(trimmed.ToLowerInvariant());
                }
            }
        }

        if (keywords.Count == 0) {
            this.logger.Warning("[LuminaDicePattern] Dynamic discovery yielded no results. Using hardcoded fallbacks.");
            string[] fallbacks = { "jetez les dés", "jette les dés", "lancer de dé", "lancer d'un dé", "obtenez", "obtient", "würfelst", "würfelt", "you roll a", "rolls a", "ダイスを振った" };
            foreach (var fb in fallbacks) keywords.Add(fb);
        }

        return keywords.ToList();
    }
}