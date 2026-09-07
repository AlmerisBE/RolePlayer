namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class MacrosConfigSubTab {
    private IMacroManagementService macroService;
    private ILocalizationService localization;
    private ITextureProvider textureProvider;

    private string newMacroName = string.Empty;

    private RoleplayMacro? selectedMacro;
    private Guid macroToDelete = Guid.Empty;
    private bool isDeleteDialogOpen = false;
    private bool isIconPickerOpen = false;

    public bool IsSidePanelOpen => this.selectedMacro != null;

    public MacrosConfigSubTab(IMacroManagementService macroService, ILocalizationService localization, ITextureProvider textureProvider) {
        this.macroService = macroService;
        this.localization = localization;
        this.textureProvider = textureProvider;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_macro_create"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(availableWidth - buttonWidth - spacing);
        ImGui.InputTextWithHint("##NewMacroName", this.localization.Translate("config_macro_name_hint"), ref this.newMacroName, 64);
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddMacro", new Vector2(buttonWidth, 0)) && !string.IsNullOrWhiteSpace(this.newMacroName)) {
            var newMacro = new RoleplayMacro {
                Name = this.newMacroName.Trim(),
                IconId = 66001 // Default macro icon (M)
            };
            this.macroService.CreateMacro(newMacro);
            this.selectedMacro = newMacro;
            this.newMacroName = string.Empty;
        }
        ImGui.PopFont();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var macros = this.macroService.GetMacros().ToList();
        if (macros.Count == 0) return;

        if (ImGui.BeginTable("MacrosTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_macro_col_icon"), ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableHeadersRow();

            foreach (var macro in macros) {
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 32f);

                bool isSelected = this.selectedMacro?.Id == macro.Id;

                ImGui.TableNextColumn();
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.0f, 0.5f));
                float selectableHeight = 32f - (ImGui.GetStyle().CellPadding.Y * 2);

                if (ImGui.Selectable($"{macro.Name}##sel_{macro.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, selectableHeight))) this.selectedMacro = isSelected ? null : macro;
                ImGui.PopStyleVar();

                ImGui.TableNextColumn();
                this.DrawIconPreview(macro.IconId, 24f);

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##Del_{macro.Id}")) {
                    this.macroToDelete = macro.Id;
                    this.isDeleteDialogOpen = true;
                }
                ImGui.PopFont();
                ImGui.PopStyleColor();
            }
            ImGui.EndTable();
        }

        this.DrawDeleteConfirmationModal();
    }

    public void DrawSidePanel() {
        if (this.selectedMacro == null) return;

        bool changed = false;

        string closeIcon = FontAwesomeIcon.Times.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        if (ImGui.BeginTable("MacroSettingsHeaderTable", 2)) {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("CloseBtn", ImGuiTableColumnFlags.WidthFixed, closeBtnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.SetWindowFontScale(1.3f);

            string title = string.IsNullOrWhiteSpace(this.selectedMacro.Name) ? this.localization.Translate("config_macro_settings") : this.selectedMacro.Name;
            ImGui.TextUnformatted(title);
            ImGui.SetWindowFontScale(1.0f);

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{closeIcon}##CloseMacroDetails")) {
                this.selectedMacro = null;
                ImGui.PopFont();
                ImGui.EndTable();
                return;
            }
            ImGui.PopFont();

            ImGui.EndTable();
        }

        ImGui.Separator();
        ImGui.Spacing();

        string name = this.selectedMacro.Name;
        if (ImGui.InputText(this.localization.Translate("config_common_name"), ref name, 64)) {
            this.selectedMacro.Name = name;
            changed = true;
        }

        ImGui.Spacing();
        ImGui.TextDisabled(this.localization.Translate("config_macro_col_icon"));

        bool openIconPicker = false;

        if (ImGui.BeginTable("MacroIconTable", 2, ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn("IconPreview", ImGuiTableColumnFlags.WidthFixed, 42f);
            ImGui.TableSetupColumn("IconSelect", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            this.DrawIconPreview(this.selectedMacro.IconId, 42f);

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();

            // Capture le clic sans ouvrir la popup directement dans la table
            if (ImGui.Button(this.localization.Translate("config_macro_icon_select"), new Vector2(-1, 42f))) openIconPicker = true;

            ImGui.EndTable();
        }

        // Ouvre la popup dans le même contexte ID que le BeginPopup
        if (openIconPicker) ImGui.OpenPopup("IconPickerPopup");

        this.DrawIconPickerPopup(ref changed);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextDisabled(this.localization.Translate("config_macro_col_content"));
        ImGui.Spacing();

        string content = this.selectedMacro.Content;
        float inputHeight = ImGui.GetTextLineHeight() * 16f;

        if (ImGui.InputTextMultiline("##MacroContent", ref content, 2048, new Vector2(-1, inputHeight))) {
            this.selectedMacro.Content = content;
            changed = true;
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_lines_hint"));

        if (changed) {
            this.macroService.UpdateMacro(this.selectedMacro.Id, this.selectedMacro.Name, this.selectedMacro.Content, this.selectedMacro.IconId);
        }
    }

    private void DrawIconPreview(uint iconId, float size) {
        try {
            var lookup = new GameIconLookup { IconId = iconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null) ImGui.Image(iconWrap.Handle, new Vector2(size, size));
            else ImGui.Dummy(new Vector2(size, size));
        }
        catch (IconNotFoundException) {
            ImGui.Dummy(new Vector2(size, size));
        }
    }

    private void DrawIconPickerPopup(ref bool changed) {
        ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.FirstUseEver);

        if (ImGui.BeginPopup("IconPickerPopup")) {
            ImGui.TextDisabled(this.localization.Translate("config_macro_icon_picker"));
            ImGui.Separator();

            if (ImGui.BeginChild("IconGrid", new Vector2(0, 0), false, ImGuiWindowFlags.AlwaysVerticalScrollbar)) {
                int columns = (int)(ImGui.GetContentRegionAvail().X / 46f);
                if (columns < 1) columns = 1;

                if (ImGui.BeginTable("IconGridTable", columns, ImGuiTableFlags.SizingFixedFit)) {
                    // Standard FFXIV macro icons range from 66001 to approx 66344
                    for (uint iconId = 66001; iconId <= 66344; iconId++) {
                        if ((iconId - 66001) % columns == 0) ImGui.TableNextRow();

                        ImGui.TableNextColumn();

                        try {
                            var lookup = new GameIconLookup { IconId = iconId, HiRes = false };
                            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

                            if (iconWrap != null) {
                                ImGui.PushID($"icon_{iconId}");
                                if (ImGui.ImageButton(iconWrap.Handle, new Vector2(38, 38))) {
                                    this.selectedMacro!.IconId = iconId;
                                    changed = true;
                                    ImGui.CloseCurrentPopup();
                                }
                                ImGui.PopID();
                            }
                        }
                        catch (IconNotFoundException) { }
                    }
                    ImGui.EndTable();
                }
                ImGui.EndChild();
            }
            ImGui.EndPopup();
        }
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
                if (this.selectedMacro?.Id == this.macroToDelete) this.selectedMacro = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
}