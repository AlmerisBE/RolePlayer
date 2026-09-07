namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;

public class EmoteExecutionProvider : IEmoteExecutionService {
    private const uint EmoteWindowCommandId = 17;

    private readonly HashSet<uint> emotesWithVariations = new() { 50, 52, 53, 174 };
    private Dictionary<uint, DateTime> lastExecutionTime = new();
    private uint lastPersistentEmoteId = 0;

    private IDataManager dataManager;
    private IPlayerStateProvider playerStateProvider;
    private ILoggerService logger;
    private INativeExecutionService nativeExecution;

    public EmoteExecutionProvider(
        IDataManager dataManager,
        IPlayerStateProvider playerStateProvider,
        ILoggerService logger,
        INativeExecutionService nativeExecution) {

        this.dataManager = dataManager;
        this.playerStateProvider = playerStateProvider;
        this.logger = logger;
        this.nativeExecution = nativeExecution;
    }

    public void ExecuteEmote(uint emoteId) {
        this.logger.Debug($"Attempting to execute emote ID: {emoteId}");

        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) return;

        var emoteRow = emoteSheet.GetRowOrDefault(emoteId);
        if (!emoteRow.HasValue) return;

        if (emoteRow.Value.UnlockLink != 0 && !this.playerStateProvider.IsEmoteUnlocked(emoteId)) {
            this.logger.Warning($"Emote {emoteId} is locked. Execution aborted.");
            return;
        }

        if (this.emotesWithVariations.Contains(emoteId)) {
            uint activeId = this.playerStateProvider.GetActiveEmoteId();
            bool isTransitioning = this.lastExecutionTime.TryGetValue(emoteId, out var lastTime) && (DateTime.Now - lastTime).TotalSeconds < 2.5;

            if ((activeId != 0 || isTransitioning) && this.lastPersistentEmoteId == emoteId) {
                this.logger.Debug($"Emote {emoteId} variation sequence detected. Injecting /cpose variation command.");
                this.nativeExecution.Execute("/cpose");
                this.lastExecutionTime[emoteId] = DateTime.Now;
                return;
            }

            this.lastPersistentEmoteId = emoteId;
        }
        else {
            this.lastPersistentEmoteId = 0;
        }

        var textCommandRef = emoteRow.Value.TextCommand;
        if (!textCommandRef.IsValid) return;

        var command = textCommandRef.Value.Command.ToString();
        if (string.IsNullOrEmpty(command)) return;

        this.nativeExecution.Execute($"{command} motion");
        this.lastExecutionTime[emoteId] = DateTime.Now;
    }

    public unsafe void OpenNativeEmoteWindow() {
        try {
            var uiModule = UIModule.Instance();
            if (uiModule != null) uiModule->ExecuteMainCommand(EmoteWindowCommandId);
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Failed to open native Emote window.");
        }
    }
}