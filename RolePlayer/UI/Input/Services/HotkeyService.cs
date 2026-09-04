namespace RolePlayer.UI.Input.Services;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.Input.Contracts;
using System;

public class HotkeyService : IHotkeyService, IDisposable {
    private IKeyState keyState;
    private IFramework framework;
    private IConfigurationService configService;
    private bool wasKeyPressed = false;

    public event Action? OnHotkeyPressed;

    public HotkeyService(IKeyState keyState, IFramework framework, IConfigurationService configService) {
        this.keyState = keyState;
        this.framework = framework;
        this.configService = configService;

        this.framework.Update += this.OnUpdate;
    }

    private unsafe void OnUpdate(IFramework fw) {
        var config = this.configService.GetConfig();
        var targetKey = config.Hotkey;

        if (targetKey == 0) {
            return;
        }

        bool isKeyPressed = this.keyState[targetKey];
        bool isInputFocused = false;

        try {
            if (ImGui.GetIO().WantCaptureKeyboard) {
                isInputFocused = true;
            }

            var uiModule = UIModule.Instance();
            if (uiModule != null) {
                var raptureAtkModule = uiModule->GetRaptureAtkModule();
                if (raptureAtkModule != null && raptureAtkModule->AtkModule.IsTextInputActive()) {
                    isInputFocused = true;
                }
            }
        }
        catch { }

        if (isKeyPressed && !this.wasKeyPressed && !isInputFocused) {
            bool ctrlPressed = this.keyState[VirtualKey.CONTROL];
            bool shiftPressed = this.keyState[VirtualKey.SHIFT];
            bool altPressed = this.keyState[VirtualKey.MENU];

            if (ctrlPressed == config.HotkeyCtrl && shiftPressed == config.HotkeyShift && altPressed == config.HotkeyAlt) {
                this.OnHotkeyPressed?.Invoke();
            }
        }

        this.wasKeyPressed = isKeyPressed;
    }

    public void Dispose() {
        this.framework.Update -= this.OnUpdate;
    }
}