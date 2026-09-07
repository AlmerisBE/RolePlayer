namespace RolePlayer.UI.MainWindow.Components.Macros;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class MacroDeleteModalComponent {
    private IMacroManagementService macroService;
    private IGroupManagementService groupService;
    private ITagManagementService tagService;
    private ILocalizationService localization;
    private IMacroSelectionState selectionState;

    private Guid macroToDelete = Guid.Empty;
    private bool isDeleteDialogOpen = false;

    public MacroDeleteModalComponent(
        IMacroManagementService macroService,
        IGroupManagementService groupService,
        ITagManagementService tagService,
        ILocalizationService localization,
        IMacroSelectionState selectionState) {

        this.macroService = macroService;
        this.groupService = groupService;
        this.tagService = tagService;
        this.localization = localization;
        this.selectionState = selectionState;
    }

    public void Open(Guid macroId) {
        this.macroToDelete = macroId;
        this.isDeleteDialogOpen = true;
    }

    public void Draw() {
        if (this.isDeleteDialogOpen) {
            ImGui.OpenPopup(this.localization.Translate("config_macro_del_title"));
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal(this.localization.Translate("config_macro_del_title"), ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            var macroName = this.macroService.GetMacros().FirstOrDefault(m => m.Id == this.macroToDelete)?.Name ?? "Unknown";
            ImGui.Text(this.localization.Translate("config_macro_del_desc", macroName));
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), this.localization.Translate("config_macro_del_warn"));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.localization.Translate("config_common_yes_delete"), new Vector2(120, 0))) {
                this.groupService.RemoveMacroFromGroup(this.macroToDelete);

                var macroTags = this.tagService.GetTagsForMacro(this.macroToDelete).ToList();
                foreach (var tag in macroTags) {
                    this.tagService.RemoveTagFromMacro(this.macroToDelete, tag);
                }

                this.macroService.DeleteMacro(this.macroToDelete);
                if (this.selectionState.SelectedMacro?.Id == this.macroToDelete) this.selectionState.SelectedMacro = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
}