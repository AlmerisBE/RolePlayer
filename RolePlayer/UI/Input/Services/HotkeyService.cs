namespace RolePlayer.UI.Input.Services;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Input.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class HotkeyService : IHotkeyService, IDisposable {
    private IKeyState keyState;
    private IDalamudPluginInterface pluginInterface;
    private IConfigurationService configService;
    private IEmoteExecutionService emoteExecution;
    private IMacroExecutionService macroExecution;
    private IMacroManagementService macroManagement;

    private HashSet<VirtualKey> pressedKeys = new();
    private bool mainUiKeyPressed = false;
    private bool isSuspended = false;

    public event Action? OnHotkeyPressed;

    public HotkeyService(
        IKeyState keyState,
        IDalamudPluginInterface pluginInterface,
        IConfigurationService configService,
        IEmoteExecutionService emoteExecution,
        IMacroExecutionService macroExecution,
        IMacroManagementService macroManagement) {

        this.keyState = keyState;
        this.pluginInterface = pluginInterface;
        this.configService = configService;
        this.emoteExecution = emoteExecution;
        this.macroExecution = macroExecution;
        this.macroManagement = macroManagement;

        this.pluginInterface.UiBuilder.Draw += this.OnDraw;
    }

    public void Suspend() {
        this.isSuspended = true;
    }

    public void Resume() {
        this.isSuspended = false;
        this.pressedKeys.Clear();

        foreach (var key in this.keyState.GetValidVirtualKeys()) {
            if (this.keyState[key]) {
                this.pressedKeys.Add(key);
            }
        }

        var config = this.configService.GetConfig();
        this.mainUiKeyPressed = config.Hotkey != 0 && this.keyState[config.Hotkey];
    }

    public void RegisterHotkey(KeyCombination key, ActionReference action) {
        var profile = this.configService.GetCurrentProfile();
        var existing = profile.Hotkeys.FirstOrDefault(h => h.Key.Equals(key));

        if (existing != null) {
            existing.Action = action;
        }
        else {
            profile.Hotkeys.Add(new HotkeyBinding { Key = key, Action = action });
        }

        this.configService.Save();

        if (this.keyState[key.Key]) {
            this.pressedKeys.Add(key.Key);
        }
    }

    public void UnregisterHotkey(KeyCombination key) {
        var profile = this.configService.GetCurrentProfile();
        if (profile.Hotkeys.RemoveAll(h => h.Key.Equals(key)) > 0) this.configService.Save();
    }

    public KeyCombination? GetAssignedKey(ActionReference action) {
        var profile = this.configService.GetCurrentProfile();
        return profile.Hotkeys.FirstOrDefault(h => h.Action.Equals(action))?.Key;
    }

    public ActionReference? GetAssignedAction(KeyCombination key) {
        var profile = this.configService.GetCurrentProfile();
        return profile.Hotkeys.FirstOrDefault(h => h.Key.Equals(key))?.Action;
    }

    private unsafe bool IsInputFocused() {
        try {
            // Remplacement critique : on ne bloque plus sur le focus de fenêtre, mais uniquement sur l'édition de texte ImGui
            if (ImGui.GetIO().WantTextInput) return true;

            var uiModule = UIModule.Instance();
            if (uiModule != null) {
                var raptureAtkModule = uiModule->GetRaptureAtkModule();
                if (raptureAtkModule != null && raptureAtkModule->AtkModule.IsTextInputActive()) return true;
            }
        }
        catch { }

        return false;
    }

    private void OnDraw() {
        if (this.IsInputFocused() || this.isSuspended) {
            this.CleanupPressedKeys();
            return;
        }

        this.CheckMainUiHotkey();
        this.CheckActionHotkeys();
        this.CleanupPressedKeys();
    }

    private void CheckMainUiHotkey() {
        var config = this.configService.GetConfig();
        if (config.Hotkey == 0) return;

        bool isCtrl = this.keyState[VirtualKey.CONTROL];
        bool isShift = this.keyState[VirtualKey.SHIFT];
        bool isAlt = this.keyState[VirtualKey.MENU];

        if (config.HotkeyCtrl == isCtrl && config.HotkeyShift == isShift && config.HotkeyAlt == isAlt) {
            bool isKeyDown = this.keyState[config.Hotkey];

            if (isKeyDown && !this.mainUiKeyPressed) {
                this.OnHotkeyPressed?.Invoke();
                this.mainUiKeyPressed = true;
            }
            else if (!isKeyDown) {
                this.mainUiKeyPressed = false;
            }
        }
        else {
            this.mainUiKeyPressed = false;
        }
    }

    private void CheckActionHotkeys() {
        var profile = this.configService.GetCurrentProfile();

        bool isCtrl = this.keyState[VirtualKey.CONTROL];
        bool isShift = this.keyState[VirtualKey.SHIFT];
        bool isAlt = this.keyState[VirtualKey.MENU];

        foreach (var binding in profile.Hotkeys) {
            if (binding.Key.Ctrl != isCtrl || binding.Key.Shift != isShift || binding.Key.Alt != isAlt) continue;

            bool isKeyDown = this.keyState[binding.Key.Key];
            bool wasKeyDown = this.pressedKeys.Contains(binding.Key.Key);

            if (isKeyDown && !wasKeyDown) {
                this.ExecuteAction(binding.Action);
                this.pressedKeys.Add(binding.Key.Key);
            }
        }
    }

    private void ExecuteAction(ActionReference action) {
        if (action.Type == ActionType.Emote) {
            this.emoteExecution.ExecuteEmote(action.EmoteId);
        }
        else if (action.Type == ActionType.Macro) {
            var macro = this.macroManagement.GetMacros().FirstOrDefault(m => m.Id == action.MacroId);
            if (macro != null) this.macroExecution.Execute(macro);
        }
    }

    private void CleanupPressedKeys() {
        foreach (var key in this.pressedKeys.ToList()) {
            if (!this.keyState[key]) this.pressedKeys.Remove(key);
        }
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.OnDraw;
    }
}