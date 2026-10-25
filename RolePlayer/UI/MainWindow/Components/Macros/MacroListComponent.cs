namespace RolePlayer.UI.MainWindow.Components.Macros;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class MacroListComponent {
    private IMacroManagementService macroService;
    private IMacroExecutionService executionService;
    private IMacroSelectionState selectionState;
    private ILocalizationService localization;
    private ITextureProvider textureProvider;
    private MacroContextMenuComponent contextMenu;

    public MacroListComponent(
        IMacroManagementService macroService,
        IMacroExecutionService executionService,
        IMacroSelectionState selectionState,
        ILocalizationService localization,
        ITextureProvider textureProvider,
        MacroContextMenuComponent contextMenu) {

        this.macroService = macroService;
        this.executionService = executionService;
        this.selectionState = selectionState;
        this.localization = localization;
        this.textureProvider = textureProvider;
        this.contextMenu = contextMenu;
    }

    public void Draw() {
        var macros = this.macroService.GetMacros().ToList();

        if (ImGui.BeginChild("MacroListScrollArea", new Vector2(0, 0), false, ImGuiWindowFlags.None)) {
            if (ImGui.BeginTable("MacrosListTable", 3, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY)) {
                ImGui.TableSetupColumn(this.localization.Translate("browser_col_icon"), ImGuiTableColumnFlags.WidthFixed, 32f);
                ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 35f);

                foreach (var macro in macros) {
                    ImGui.TableNextRow();

                    bool isSelected = this.selectionState.SelectedMacro?.Id == macro.Id;

                    ImGui.TableNextColumn();
                    this.DrawIconPreview(macro.IconId, 24f);

                    ImGui.TableNextColumn();
                    if (ImGui.Selectable($"{macro.Name}##select_{macro.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 28f))) {
                        this.selectionState.SelectedMacro = isSelected ? null : macro;
                    }

                    this.contextMenu.Draw(macro);

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);

                    // Bascule dynamique : Bouton Stop (rouge) si le moteur est actif, sinon Play (vert)
                    if (this.executionService.IsExecuting) {
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                        if (ImGui.Button($"{FontAwesomeIcon.Stop.ToIconString()}##stop_{macro.Id}", new Vector2(-1, 28f))) {
                            this.executionService.CancelAll();
                        }
                        ImGui.PopStyleColor();
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_stop"));
                    }
                    else {
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
                        if (ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##play_{macro.Id}", new Vector2(-1, 28f))) {
                            this.executionService.Execute(macro);
                        }
                        ImGui.PopStyleColor();
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_execute"));
                    }

                    ImGui.PopFont();
                }
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
    }

    private void DrawIconPreview(uint iconId, float size) {
        if (iconId == 0) {
            ImGui.Dummy(new Vector2(size, size));
            return;
        }

        try {
            var lookup = new GameIconLookup { IconId = iconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null) {
                ImGui.Image(iconWrap.Handle, new Vector2(size, size));
            }
            else {
                ImGui.Dummy(new Vector2(size, size));
            }
        }
        catch (Exception) {
            ImGui.Dummy(new Vector2(size, size));
        }
    }
}