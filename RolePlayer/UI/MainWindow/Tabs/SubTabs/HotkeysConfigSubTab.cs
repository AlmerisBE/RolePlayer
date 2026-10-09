namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Input.Components;
using RolePlayer.UI.Input.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class HotkeysConfigSubTab {
    private IConfigurationService configService;
    private IHotkeyService hotkeyService;
    private ILocalizationService localization;
    private IEmoteCache emoteCache;
    private IMacroManagementService macroService;
    private HotkeyAssignerModal hotkeyAssignerModal;

    private bool showActionPicker = false;
    private int selectedActionType = 0; // 0 = Emote, 1 = Macro
    private uint selectedEmoteId = 0;
    private Guid selectedMacroId = Guid.Empty;

    // Champ différé pour éviter les conflits de pile d'ID ImGui
    private ActionReference? pendingActionToAssign = null;

    public string Name => this.localization.Translate("config_tab_hotkeys");
    private string PickerPopupId => $"{this.localization.Translate("config_hotkeys_add_new")}###ActionPickerPopup";

    public HotkeysConfigSubTab(
        IConfigurationService configService,
        IHotkeyService hotkeyService,
        ILocalizationService localization,
        IEmoteCache emoteCache,
        IMacroManagementService macroService,
        HotkeyAssignerModal hotkeyAssignerModal) {

        this.configService = configService;
        this.hotkeyService = hotkeyService;
        this.localization = localization;
        this.emoteCache = emoteCache;
        this.macroService = macroService;
        this.hotkeyAssignerModal = hotkeyAssignerModal;
    }

    public void Draw() {
        var profile = this.configService.GetCurrentProfile();

        ImGui.TextDisabled(this.localization.Translate("config_hotkeys_description"));
        ImGui.Spacing();

        if (ImGui.BeginTable("HotkeysListTable", 4, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_key"), ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_type"), ImGuiTableColumnFlags.WidthFixed, 80f);
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_action"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_delete"), ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableHeadersRow();

            var keysToRemove = new System.Collections.Generic.List<KeyCombination>();

            foreach (var binding in profile.Hotkeys) {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(binding.Key.ToString());

                ImGui.TableNextColumn();
                string typeStr = binding.Action.Type == ActionType.Emote
                    ? this.localization.Translate("config_hb_target_emotes")
                    : this.localization.Translate("config_hb_target_macros");
                ImGui.TextDisabled(typeStr);

                ImGui.TableNextColumn();
                string actionName = this.localization.Translate("browser_unknown");

                if (binding.Action.Type == ActionType.Emote) {
                    var emote = this.emoteCache.GetCachedEmotes().FirstOrDefault(e => e.Id == binding.Action.EmoteId);
                    if (emote != null) actionName = emote.Name;
                }
                else if (binding.Action.Type == ActionType.Macro) {
                    var macro = this.macroService.GetMacros().FirstOrDefault(m => m.Id == binding.Action.MacroId);
                    if (macro != null) actionName = macro.Name;
                }

                ImGui.TextUnformatted(actionName);

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##{binding.Key.GetHashCode()}", new Vector2(-1, 0))) {
                    keysToRemove.Add(binding.Key);
                }
                ImGui.PopFont();
            }

            ImGui.EndTable();

            foreach (var key in keysToRemove) {
                this.hotkeyService.UnregisterHotkey(key);
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button(this.localization.Translate("config_hotkeys_add_new"), new Vector2(-1, 30))) {
            this.showActionPicker = true;
            ImGui.OpenPopup(this.PickerPopupId);
        }

        this.DrawActionPickerPopup();

        // On gère l'ouverture de la modale d'écoute clavier HORS du scope du premier popup
        if (this.pendingActionToAssign != null) {
            this.hotkeyAssignerModal.Open(this.pendingActionToAssign);
            this.pendingActionToAssign = null;
        }

        this.hotkeyAssignerModal.Draw();
    }

    private void DrawActionPickerPopup() {
        ImGui.SetNextWindowSize(new Vector2(300, 0), ImGuiCond.Appearing);
        if (ImGui.BeginPopupModal(this.PickerPopupId, ref this.showActionPicker, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse)) {

            ImGui.RadioButton(this.localization.Translate("config_hb_target_emotes"), ref this.selectedActionType, 0);
            ImGui.SameLine();
            ImGui.RadioButton(this.localization.Translate("config_hb_target_macros"), ref this.selectedActionType, 1);

            ImGui.Spacing();

            ImGui.SetNextItemWidth(-1f);
            if (this.selectedActionType == 0) {
                var emotes = this.emoteCache.GetCachedEmotes().Where(e => e.IsUnlocked).ToList();
                var currentEmote = emotes.FirstOrDefault(e => e.Id == this.selectedEmoteId);
                string preview = currentEmote?.Name ?? this.localization.Translate("browser_none");

                if (ImGui.BeginCombo("##EmoteCombo", preview)) {
                    foreach (var e in emotes) {
                        if (ImGui.Selectable(e.Name, e.Id == this.selectedEmoteId)) {
                            this.selectedEmoteId = e.Id;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            else {
                var macros = this.macroService.GetMacros().ToList();
                var currentMacro = macros.FirstOrDefault(m => m.Id == this.selectedMacroId);
                string preview = currentMacro?.Name ?? this.localization.Translate("browser_none");

                if (ImGui.BeginCombo("##MacroCombo", preview)) {
                    foreach (var m in macros) {
                        if (ImGui.Selectable(m.Name, m.Id == this.selectedMacroId)) {
                            this.selectedMacroId = m.Id;
                        }
                    }
                    ImGui.EndCombo();
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            bool canProceed = (this.selectedActionType == 0 && this.selectedEmoteId != 0) || (this.selectedActionType == 1 && this.selectedMacroId != Guid.Empty);

            ImGui.BeginDisabled(!canProceed);
            if (ImGui.Button(this.localization.Translate("hotkey_assign_title"), new Vector2(-1, 30))) {

                // Au lieu d'ouvrir directement, on stocke l'action
                this.pendingActionToAssign = new ActionReference {
                    Type = this.selectedActionType == 0 ? ActionType.Emote : ActionType.Macro,
                    EmoteId = this.selectedEmoteId,
                    MacroId = this.selectedMacroId
                };

                this.showActionPicker = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndDisabled();

            ImGui.EndPopup();
        }
    }
}