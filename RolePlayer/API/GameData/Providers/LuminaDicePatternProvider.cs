namespace RolePlayer.API.GameData.Providers;

using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.API.GameEvents.Contracts;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class LuminaDicePatternProvider : IDicePatternProvider {
    private IDataManager dataManager;
    private ILoggerService logger;

    public LuminaDicePatternProvider(IDataManager dataManager, ILoggerService logger) {
        this.dataManager = dataManager;
        this.logger = logger;
    }

    public IReadOnlyList<Regex> GetLocalizedDicePatterns() {
        var patterns = new List<Regex>();

        var enSheet = this.dataManager.GetExcelSheet<LogMessage>(ClientLanguage.English);
        var localSheet = this.dataManager.GetExcelSheet<LogMessage>();

        if (localSheet == null) return patterns;

        var diceIds = new HashSet<uint> { 856, 1231, 3887, 5180 };

        if (enSheet != null) {
            foreach (var row in enSheet) {
                string rawEn = row.Text.ExtractText().ToLowerInvariant();
                if ((rawEn.Contains("random!") || rawEn.Contains("dice!")) && rawEn.Contains("roll")) {
                    diceIds.Add(row.RowId);
                }
            }
        }

        foreach (var id in diceIds) {
            var row = localSheet.GetRowOrDefault(id);
            if (!row.HasValue) continue;

            string rawLocal = row.Value.Text.ToString();
            if (string.IsNullOrWhiteSpace(rawLocal)) continue;

            // Split by control characters (payloads) to isolate pure textual parts
            var parts = Regex.Split(rawLocal, @"[\x00-\x1F]+");
            var escapedParts = parts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(Regex.Escape);

            if (!escapedParts.Any()) continue;

            // Rebuild the strict regex: Match start/end, and replace missing payloads with a lazy wildcard
            string patternStr = @"^\s*" + string.Join(".*?", escapedParts) + @"\s*$";

            try {
                var regex = new Regex(patternStr, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                patterns.Add(regex);
                this.logger.Info($"[LuminaDicePattern] Compiled strict regex for ID {id}: {patternStr}");
            }
            catch (Exception ex) {
                this.logger.Error(ex, $"[LuminaDicePattern] Failed to compile regex for ID {id}");
            }
        }

        if (patterns.Count == 0) {
            this.logger.Warning("[LuminaDicePattern] Dynamic discovery yielded no results. Using hardcoded fallbacks.");
            patterns.Add(new Regex(@"^you roll a \d+.*$", RegexOptions.IgnoreCase | RegexOptions.Compiled));
            patterns.Add(new Regex(@"^vous jetez les dés et obtenez \d+.*$", RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        return patterns;
    }
}