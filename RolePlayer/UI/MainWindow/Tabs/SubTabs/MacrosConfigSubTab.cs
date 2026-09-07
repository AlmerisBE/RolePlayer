namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class MacrosConfigSubTab {
    private IMacroManagementService macroService;
    private ILocalizationService localization;

    private string newMacroName = string.Empty;
    private string newMacroContent = string.Empty;
    private int newMacroIconId = 0;

    private Guid editingMacroId = Guid.Empty;
    private string editName = string.Empty;
    private string editContent = string.Empty;
    private int editIconId = 0;

    private Guid macroToDelete = Guid.Empty;
    private bool isDeleteDialogOpen = false;

    public MacrosConfigSubTab(IMacroManagementService macroService, ILocalizationService localization) {
        this.macroService = macroService;
        this.localization = localization;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_macro_create"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float remainingWidth = availableWidth - buttonWidth - (spacing * 3);

        ImGui.SetNextItemWidth(remainingWidth * 0.25f);
        ImGui.InputTextWithHint("##NewMacroName", this.localization.Translate("config_macro_name_hint"), ref this.newMacroName, 64);
        ImGui.SameLine();

        ImGui.SetNextItemWidth(remainingWidth * 0.15f);
        ImGui.InputInt("##NewMacroIcon", ref this.newMacroIconId, 0, 0);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_icon_hint"));
        if (this.newMacroIconId < 0) this.newMacroIconId = 0;
        ImGui.SameLine();

        ImGui.SetNextItemWidth(remainingWidth * 0.60f);
        ImGui.InputTextWithHint("##NewMacroContent", this.localization.Translate("config_macro_content_hint"), ref this.newMacroContent, 1024);
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddMacro", new Vector2(buttonWidth, 0)) && !string.IsNullOrWhiteSpace(this.newMacroName)) {
            this.macroService.CreateMacro(new RoleplayMacro {
                Name = this.newMacroName.Trim(),
                Content = this.newMacroContent.Trim(),
                IconId = (uint)this.newMacroIconId
            });
            this.newMacroName = string.Empty;
            this.newMacroContent = string.Empty;
            this.newMacroIconId = 0;
        }
        ImGui.PopFont();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var macros = this.macroService.GetMacros().ToList();
        if (macros.Count == 0) return;

        if (ImGui.BeginTable("MacrosTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn(this.localization.Translate("config_macro_col_icon"), ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableSetupColumn(this.localization.Translate("config_macro_col_content"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 75f);
            ImGui.TableHeadersRow();

            foreach (var macro in macros) {
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 28f);

                if (this.editingMacroId == macro.Id) {
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    ImGui.InputText($"##EditName_{macro.Id}", ref this.editName, 64);

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    ImGui.InputInt($"##EditIcon_{macro.Id}", ref this.editIconId, 0, 0);
                    if (this.editIconId < 0) this.editIconId = 0;

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    ImGui.InputText($"##EditContent_{macro.Id}", ref this.editContent, 1024);

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Save.ToIconString()}##Save_{macro.Id}")) {
                        this.macroService.UpdateMacro(macro.Id, this.editName.Trim(), this.editContent.Trim(), (uint)this.editIconId);
                        this.editingMacroId = Guid.Empty;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"{FontAwesomeIcon.Times.ToIconString()}##Cancel_{macro.Id}")) this.editingMacroId = Guid.Empty;
                    ImGui.PopFont();
                }
                else {
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(macro.Name);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(macro.IconId.ToString());

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    float availableCellWidth = ImGui.GetContentRegionAvail().X;
                    Vector2 textSize = ImGui.CalcTextSize(macro.Content);
                    ImGui.TextUnformatted(macro.Content);
                    if (textSize.X > availableCellWidth && ImGui.IsItemHovered()) ImGui.SetTooltip(macro.Content);

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Edit.ToIconString()}##Edit_{macro.Id}")) {
                        this.editingMacroId = macro.Id;
                        this.editName = macro.Name;
                        this.editContent = macro.Content;
                        this.editIconId = (int)macro.IconId;
                    }
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##Del_{macro.Id}")) {
                        this.macroToDelete = macro.Id;
                        this.isDeleteDialogOpen = true;
                    }
                    ImGui.PopStyleColor();
                    ImGui.PopFont();
                }
            }
            ImGui.EndTable();
        }

        this.DrawDeleteConfirmationModal();
    }

    private void DrawDeleteConfirmationModal() {
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
                this.macroService.DeleteMacro(this.macroToDelete);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
}