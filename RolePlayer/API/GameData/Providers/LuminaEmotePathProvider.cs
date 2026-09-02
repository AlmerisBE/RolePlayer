namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.API.Penumbra.Contracts;
using System.Collections.Generic;

public class LuminaEmotePathProvider : IEmotePathProvider {
    private IDataManager dataManager;
    private IObjectTable objectTable;

    public LuminaEmotePathProvider(IDataManager dataManager, IObjectTable objectTable) {
        this.dataManager = dataManager;
        this.objectTable = objectTable;
    }

    public IEnumerable<string> GetEmoteGamePaths(uint emoteId) {
        var paths = new HashSet<string>();

        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return paths;
        }

        var emoteRow = emoteSheet.GetRowOrDefault(emoteId);
        if (!emoteRow.HasValue) {
            return paths;
        }

        // Boucle sur TOUTES les timelines associées (intro, boucle de danse, etc.)
        foreach (var actionTimelineRef in emoteRow.Value.ActionTimeline) {
            if (!actionTimelineRef.IsValid) {
                continue;
            }

            var key = actionTimelineRef.Value.Key.ToString();
            if (string.IsNullOrEmpty(key)) {
                continue;
            }

            // Fichier Timeline générique
            paths.Add($"chara/action/{key}.tmb");

            // Fichier d'animation spécifique à la race/genre
            var papPath = this.GetPapPathForLocalPlayer(key);
            if (!string.IsNullOrEmpty(papPath)) {
                paths.Add(papPath);
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

        // 0 = Homme (+1), 1 = Femme (+4)
        charaCode += (gender == 0) ? 1 : 4;

        return $"chara/human/c{charaCode:D4}/animation/a0001/bt_common/{actionKey}.pap";
    }
}