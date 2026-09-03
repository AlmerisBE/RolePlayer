namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Lumina.Excel.Sheets;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;

public class EmoteExecutionProvider : IEmoteExecutionService {
    private const uint EmoteWindowCommandId = 17;

    private readonly HashSet<uint> emotesWithVariations = new() { 50, 52, 53, 174 };
    private Dictionary<uint, DateTime> lastExecutionTime = new();

    private IDataManager dataManager;
    private IPlayerStateProvider playerStateProvider;
    private ILoggerService logger;

    public EmoteExecutionProvider(IDataManager dataManager, IPlayerStateProvider playerStateProvider, ILoggerService logger) {
        this.dataManager = dataManager;
        this.playerStateProvider = playerStateProvider;
        this.logger = logger;
    }

    public unsafe void ExecuteEmote(uint emoteId) {
        this.logger.Debug($"Attempting to execute emote ID: {emoteId}");

        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return;
        }

        var emoteRow = emoteSheet.GetRowOrDefault(emoteId);
        if (!emoteRow.HasValue) {
            return;
        }

        if (emoteRow.Value.UnlockLink != 0 && !this.playerStateProvider.IsEmoteUnlocked(emoteId)) {
            this.logger.Warning($"Emote {emoteId} is locked. Execution aborted.");
            return;
        }

        // Évaluation de l'état actif ou d'un spam-clic durant la transition d'animation
        if (this.emotesWithVariations.Contains(emoteId)) {
            bool isActive = this.playerStateProvider.IsEmoteActive(emoteId);
            bool isSpam = this.lastExecutionTime.TryGetValue(emoteId, out var lastTime) && (DateTime.Now - lastTime).TotalSeconds < 1.5;

            if (isActive || isSpam) {
                this.logger.Debug($"Emote {emoteId} is already active or transitioning. Injecting /cpose variation command.");
                this.ExecuteCommand("/cpose");
                this.lastExecutionTime[emoteId] = DateTime.Now;
                return;
            }
        }

        var textCommandRef = emoteRow.Value.TextCommand;
        if (!textCommandRef.IsValid) {
            return;
        }

        var command = textCommandRef.Value.Command.ToString();
        if (string.IsNullOrEmpty(command)) {
            return;
        }

        this.ExecuteCommand($"{command} motion");
        this.lastExecutionTime[emoteId] = DateTime.Now;
    }

    private unsafe void ExecuteCommand(string commandText) {
        var raptureShellModule = RaptureShellModule.Instance();
        if (raptureShellModule == null) {
            return;
        }

        var message = new Utf8String();
        message.Ctor();
        message.SetString(commandText);

        var macro = new RaptureMacroModule.Macro();
        macro.Lines[0] = message;

        try {
            raptureShellModule->ExecuteMacro(&macro);
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Exception thrown during RaptureShellModule invocation.");
        }
        finally {
            message.Dtor();
        }
    }

    public unsafe void OpenNativeEmoteWindow() {
        try {
            var uiModule = UIModule.Instance();
            if (uiModule != null) {
                uiModule->ExecuteMainCommand(EmoteWindowCommandId);
            }
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Failed to open native Emote window.");
        }
    }
}