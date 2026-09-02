namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.API.Penumbra.Contracts;
using System.Linq;

public class LuminaEmotePathProvider : IEmotePathProvider {
    private IDataManager dataManager;

    public LuminaEmotePathProvider(IDataManager dataManager) {
        this.dataManager = dataManager;
    }

    public string GetEmoteGamePath(uint emoteId) {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return string.Empty;
        }

        var emoteRow = emoteSheet.GetRowOrDefault(emoteId);
        if (!emoteRow.HasValue) {
            return string.Empty;
        }

        // Avec Lumina v10, nous récupérons la première entrée du tableau de références
        var actionTimeline = emoteRow.Value.ActionTimeline.FirstOrDefault();
        if (!actionTimeline.IsValid) {
            return string.Empty;
        }

        var key = actionTimeline.Value.Key.ToString();
        if (string.IsNullOrEmpty(key)) {
            return string.Empty;
        }

        if (!key.Contains(".pap")) {
            return $"chara/action/{key}.pap";
        }

        return key;
    }
}