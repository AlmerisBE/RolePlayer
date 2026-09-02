namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class LuminaEmoteDebugService : IEmoteDebugService {
    private IDataManager dataManager;
    private ILoggerService logger;
    private IEmotePathProvider pathProvider;

    public LuminaEmoteDebugService(IDataManager dataManager, ILoggerService logger, IEmotePathProvider pathProvider) {
        this.dataManager = dataManager;
        this.logger = logger;
        this.pathProvider = pathProvider;
    }

    public void LogEmoteDetails(uint emoteId) {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return;
        }

        var emote = emoteSheet.GetRowOrDefault(emoteId);
        if (!emote.HasValue) {
            return;
        }

        this.logger.Info($"--- DEBUG EMOTE {emoteId} ({emote.Value.Name}) ---");

        foreach (var timeline in emote.Value.ActionTimeline) {
            if (timeline.IsValid) {
                this.logger.Info($"Timeline Key: {timeline.Value.Key}");
            }
        }

        var generatedPaths = this.pathProvider.GetEmoteGamePaths(emoteId);
        foreach (var path in generatedPaths) {
            this.logger.Info($"Generated Path: {path}");
        }
    }
}