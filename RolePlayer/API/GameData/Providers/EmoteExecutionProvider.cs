namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;

public class EmoteExecutionProvider : IEmoteExecutionService {
    private IDataManager dataManager;
    private IGameInteropProvider interopProvider;
    private IPlayerStateProvider playerStateProvider;

    private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);

    // Signature mémoire standard de Dalamud pour l'injection native de chat
    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9 74 08 41 8B C0")]
    private ProcessChatBoxDelegate? processChatBox = null;

    public EmoteExecutionProvider(IDataManager dataManager, IGameInteropProvider interopProvider, IPlayerStateProvider playerStateProvider) {
        this.dataManager = dataManager;
        this.interopProvider = interopProvider;
        this.playerStateProvider = playerStateProvider;

        this.interopProvider.InitializeFromAttributes(this);
    }

    public unsafe void ExecuteEmote(uint emoteId) {
        var emoteSheet = this.dataManager.GetExcelSheet<Emote>();
        if (emoteSheet == null) {
            return;
        }

        var emoteRow = emoteSheet.GetRowOrDefault(emoteId);
        if (!emoteRow.HasValue) {
            return;
        }

        if (emoteRow.Value.UnlockLink != 0 && !this.playerStateProvider.IsEmoteUnlocked(emoteId)) {
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

        var uiModule = (IntPtr)UIModule.Instance();
        if (uiModule == IntPtr.Zero || this.processChatBox == null) {
            return;
        }

        var message = new Utf8String();
        message.Ctor();

        message.SetString(command);

        this.processChatBox.Invoke(uiModule, (IntPtr)(&message), IntPtr.Zero, 0);

        message.Dtor();
    }
}