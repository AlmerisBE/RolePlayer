namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;

public class EmoteExecutionProvider : IEmoteExecutionService {
    private IDataManager dataManager;
    private IGameInteropProvider interopProvider;
    private IPlayerStateProvider playerStateProvider;
    private ILoggerService logger;

    private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);

    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9 74 08 41 8B C0")]
    private ProcessChatBoxDelegate? processChatBox = null;

    public EmoteExecutionProvider(
        IDataManager dataManager,
        IGameInteropProvider interopProvider,
        IPlayerStateProvider playerStateProvider,
        ILoggerService logger) {

        this.dataManager = dataManager;
        this.interopProvider = interopProvider;
        this.playerStateProvider = playerStateProvider;
        this.logger = logger;

        this.interopProvider.InitializeFromAttributes(this);

        if (this.processChatBox == null) {
            this.logger.Error("Failed to resolve ProcessChatBox signature!");
        }
        else {
            this.logger.Info("Successfully resolved ProcessChatBox signature.");
        }
    }

    public unsafe void ExecuteEmote(uint emoteId) {
        this.logger.Debug($"Attempting to execute emote ID: {emoteId}");

        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            this.logger.Error("Emote Excel sheet is null.");
            return;
        }

        var emoteRow = emoteSheet.GetRowOrDefault(emoteId);
        if (!emoteRow.HasValue) {
            this.logger.Error($"Emote ID {emoteId} not found in Excel sheet.");
            return;
        }

        if (emoteRow.Value.UnlockLink != 0 && !this.playerStateProvider.IsEmoteUnlocked(emoteId)) {
            this.logger.Warning($"Emote {emoteId} is locked. Execution aborted.");
            return;
        }

        var textCommandRef = emoteRow.Value.TextCommand;
        if (!textCommandRef.IsValid) {
            this.logger.Error($"Emote {emoteId} has no valid TextCommand reference.");
            return;
        }

        var command = textCommandRef.Value.Command.ToString();
        if (string.IsNullOrEmpty(command)) {
            this.logger.Error($"Emote {emoteId} TextCommand string is empty.");
            return;
        }

        var finalCommand = $"{command} motion";
        this.logger.Debug($"Constructed command: '{finalCommand}'");

        var uiModule = (IntPtr)UIModule.Instance();
        if (uiModule == IntPtr.Zero) {
            this.logger.Error("UIModule.Instance() returned IntPtr.Zero.");
            return;
        }

        if (this.processChatBox == null) {
            this.logger.Error("ProcessChatBox delegate is null. Cannot execute command.");
            return;
        }

        this.logger.Debug("Allocating Utf8String for native call...");
        var message = new Utf8String();
        message.Ctor();
        message.SetString(finalCommand);

        try {
            this.logger.Debug("Invoking native ProcessChatBox...");
            this.processChatBox.Invoke(uiModule, (IntPtr)(&message), IntPtr.Zero, 0);
            this.logger.Debug("ProcessChatBox invoked successfully.");
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Exception thrown during ProcessChatBox invocation.");
        }
        finally {
            message.Dtor();
        }
    }
}