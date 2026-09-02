namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Lumina.Excel.Sheets;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;

public class EmoteExecutionProvider : IEmoteExecutionService {
    private IDataManager dataManager;
    private IPlayerStateProvider playerStateProvider;
    private ILoggerService logger;

    public EmoteExecutionProvider(
        IDataManager dataManager,
        IPlayerStateProvider playerStateProvider,
        ILoggerService logger) {

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

        // Security check: ensure the local player actually unlocked the emote
        if (emoteRow.Value.UnlockLink != 0 && !this.playerStateProvider.IsEmoteUnlocked(emoteId)) {
            this.logger.Warning($"Emote {emoteId} is locked. Execution aborted.");
            return;
        }

        var textCommandRef = emoteRow.Value.TextCommand;
        if (!textCommandRef.IsValid) {
            return;
        }

        var command = textCommandRef.Value.Command.ToString();
        if (string.IsNullOrEmpty(command)) {
            return;
        }

        var finalCommand = $"{command} motion";
        this.logger.Debug($"Constructed command: '{finalCommand}'");

        var raptureShellModule = RaptureShellModule.Instance();
        if (raptureShellModule == null) {
            this.logger.Error("RaptureShellModule.Instance() returned null.");
            return;
        }

        this.logger.Debug("Allocating Utf8String and Macro struct...");

        var message = new Utf8String();
        message.Ctor();
        message.SetString(finalCommand);

        var macro = new RaptureMacroModule.Macro();

        macro.Lines[0] = message;

        try {
            this.logger.Debug("Invoking native RaptureShellModule->ExecuteMacro...");

            raptureShellModule->ExecuteMacro(&macro);

            this.logger.Debug("RaptureShellModule invoked successfully.");
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Exception thrown during RaptureShellModule invocation.");
        }
        finally {
            message.Dtor();
        }
    }
}