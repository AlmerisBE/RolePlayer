namespace RolePlayer.UI.Input.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Input.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System.Linq;
using System.Numerics;

public class HotkeyAssignerModal {
    private IKeyState keyState;
    private IHotkeyService hotkeyService;
    private ILocalizationService localization;
    private IEmoteCache emoteCache;
    private IMacroManagementService macroService;

    private bool isOpen = false;
    private ActionReference? currentAction;
    private KeyCombination? capturedKey;
    private ActionReference? conflictingAction;

    private string PopupId => $"{this.localization.Translate("hotkey_assign_title")}###AssignHotkeyPopup";

    public HotkeyAssignerModal(
        IKeyState keyState,
        IHotkeyService hotkeyService,
        ILocalizationService localization,
        IEmoteCache emoteCache,
        IMacroManagementService macroService) {

        this.keyState = keyState;
        this.hotkeyService = hotkeyService;
        this.localization = localization;
        this.emoteCache = emoteCache;
        this.macroService = macroService;
    }

    public void Open(ActionReference action) {
        this.currentAction = action;
        this.capturedKey = null;
        this.conflictingAction = null;
        this.isOpen = true;
        this.hotkeyService.Suspend();
        ImGui.OpenPopup(this.PopupId);
    }

    private void Close() {
        this.isOpen = false;
        ImGui.CloseCurrentPopup();
    }

    public void Draw() {
        if (!this.isOpen) return;

        ImGui.SetNextWindowSize(new Vector2(350, 0), ImGuiCond.Always);

        bool wasOpen = this.isOpen;
        if (ImGui.BeginPopupModal(this.PopupId, ref this.isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse)) {
            if (this.capturedKey == null) {
                ImGui.TextUnformatted(this.localization.Translate("hotkey_listening"));
                ImGui.Spacing();

                var key = this.GetPressedCombination();
                if (key != null) {
                    this.capturedKey = key;
                    this.CheckForConflicts();
                }
            }
            else {
                ImGui.TextDisabled(this.capturedKey.ToString());
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                if (this.conflictingAction != null && !this.conflictingAction.Equals(this.currentAction)) {
                    string conflictName = this.GetActionName(this.conflictingAction);
                    ImGui.TextWrapped(this.localization.Translate("hotkey_conflict", conflictName));
                    ImGui.Spacing();

                    if (ImGui.Button(this.localization.Translate("hotkey_confirm"), new Vector2(120, 0))) {
                        this.hotkeyService.RegisterHotkey(this.capturedKey, this.currentAction!);
                        this.Close();
                    }
                    ImGui.SameLine();
                }
                else {
                    this.hotkeyService.RegisterHotkey(this.capturedKey, this.currentAction!);
                    this.Close();
                }

                if (ImGui.Button(this.localization.Translate("hotkey_cancel"), new Vector2(120, 0))) {
                    this.Close();
                }
            }

            ImGui.EndPopup();
        }

        // Si la modale vient tout juste de se fermer, on reprend l'écoute du clavier.
        if (wasOpen && !this.isOpen) {
            this.hotkeyService.Resume();
        }
    }

    private void CheckForConflicts() {
        if (this.capturedKey == null) return;

        var existingAction = this.hotkeyService.GetAssignedAction(this.capturedKey);
        if (existingAction != null) {
            this.conflictingAction = existingAction;
        }
    }

    private string GetActionName(ActionReference action) {
        if (action.Type == ActionType.Emote) {
            var emote = this.emoteCache.GetCachedEmotes().FirstOrDefault(e => e.Id == action.EmoteId);
            return emote?.Name ?? this.localization.Translate("browser_unknown");
        }
        else {
            var macro = this.macroService.GetMacros().FirstOrDefault(m => m.Id == action.MacroId);
            return macro?.Name ?? this.localization.Translate("browser_unknown");
        }
    }

    private KeyCombination? GetPressedCombination() {
        bool ctrl = this.keyState[VirtualKey.CONTROL];
        bool shift = this.keyState[VirtualKey.SHIFT];
        bool alt = this.keyState[VirtualKey.MENU];

        foreach (var key in this.keyState.GetValidVirtualKeys()) {
            if (key == VirtualKey.CONTROL || key == VirtualKey.SHIFT || key == VirtualKey.MENU ||
                key == VirtualKey.LBUTTON || key == VirtualKey.RBUTTON || key == VirtualKey.MBUTTON) { continue; }

            if (this.keyState[key]) {
                return new KeyCombination { Key = key, Ctrl = ctrl, Shift = shift, Alt = alt };
            }
        }

        return null;
    }
}