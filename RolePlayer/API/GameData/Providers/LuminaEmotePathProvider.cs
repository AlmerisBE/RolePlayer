namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.API.Penumbra.Contracts;
using System;
using System.Collections.Generic;

public class LuminaEmotePathProvider : IEmotePathProvider {
    private IDataManager dataManager;
    private IObjectTable objectTable;

    public LuminaEmotePathProvider(IDataManager dataManager, IObjectTable objectTable) {
        this.dataManager = dataManager;
        this.objectTable = objectTable;
    }

    public IEnumerable<string> GetEmoteGamePaths(uint emoteId) {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return paths;
        }

        var emoteRow = emoteSheet.GetRowOrDefault(emoteId);
        if (!emoteRow.HasValue) {
            return paths;
        }

        foreach (var actionTimelineRef in emoteRow.Value.ActionTimeline) {
            if (!actionTimelineRef.IsValid) {
                continue;
            }

            var rawKey = actionTimelineRef.Value.Key.ToString();
            if (string.IsNullOrEmpty(rawKey)) {
                continue;
            }

            var baseKey = rawKey;
            if (baseKey.EndsWith("_start", StringComparison.OrdinalIgnoreCase)) {
                baseKey = baseKey.Substring(0, baseKey.Length - 6);
            }
            else if (baseKey.EndsWith("_loop", StringComparison.OrdinalIgnoreCase)) {
                baseKey = baseKey.Substring(0, baseKey.Length - 5);
            }
            else if (baseKey.EndsWith("_end", StringComparison.OrdinalIgnoreCase)) {
                baseKey = baseKey.Substring(0, baseKey.Length - 4);
            }

            var suffixes = new[] { "", "_start", "_loop", "_end" };

            foreach (var suffix in suffixes) {
                var key = $"{baseKey}{suffix}";

                paths.Add($"chara/action/{key}.tmb");

                // 1. Chemin spécifique à la race et au genre du joueur local
                var specificPapPath = this.GetPapPathForLocalPlayer(key);
                if (!string.IsNullOrEmpty(specificPapPath)) {
                    paths.Add(specificPapPath);
                    paths.Add($"Animation/Animation/{specificPapPath}");
                    paths.Add($"Animation/{specificPapPath}");
                }

                // 2. Chemin universel FFXIV de base (Midlander Male - c0101) utilisé par FFXIV comme Fallback
                var fallbackPapPath = $"chara/human/c0101/animation/a0001/bt_common/{key}.pap";
                paths.Add(fallbackPapPath);
                paths.Add($"Animation/Animation/{fallbackPapPath}");
                paths.Add($"Animation/{fallbackPapPath}");
            }
        }

        return paths;
    }

    private string GetPapPathForLocalPlayer(string actionKey) {
        var player = this.objectTable.LocalPlayer;
        if (player == null) {
            return string.Empty;
        }

        var customize = player.Customize;
        var race = customize[0];
        var gender = customize[1];
        var clan = customize[4];

        int charaCode = 0;
        switch (race) {
            case 1: charaCode = (clan == 2) ? 200 : 100; break;
            case 2: charaCode = 300; break;
            case 3: charaCode = 600; break;
            case 4: charaCode = 400; break;
            case 5: charaCode = 500; break;
            case 6: charaCode = 700; break;
            case 7: charaCode = 1100; break;
            case 8: charaCode = 1200; break;
            default: charaCode = 100; break;
        }

        charaCode += (gender == 0) ? 1 : 4;

        return $"chara/human/c{charaCode:D4}/animation/a0001/bt_common/{actionKey}.pap";
    }
}