namespace RolePlayer.UI.MainWindow.Tabs;

using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Components.Macros;
using RolePlayer.UI.MainWindow.Contracts;
using System;

public class MacrosTab : IEmoteBrowserTab, IDisposable {
    private ILocalizationService localization;
    private IMacroSelectionState selectionState;

    private MacroCreationComponent creationComponent;
    private MacroSearchComponent searchComponent;
    private MacroListComponent listComponent;
    private MacroEditorPanelComponent editorPanelComponent;

    public string TabName => this.localization.Translate("main_tab_macros");
    public int SortOrder => 20;
    public bool IsSidePanelOpen => this.selectionState.SelectedMacro != null;

    public MacrosTab(
        ILocalizationService localization,
        IMacroSelectionState selectionState,
        MacroCreationComponent creationComponent,
        MacroSearchComponent searchComponent,
        MacroListComponent listComponent,
        MacroEditorPanelComponent editorPanelComponent) {

        this.localization = localization;
        this.selectionState = selectionState;
        this.creationComponent = creationComponent;
        this.searchComponent = searchComponent;
        this.listComponent = listComponent;
        this.editorPanelComponent = editorPanelComponent;
    }

    public void Draw() {
        this.creationComponent.Draw();

        Dalamud.Bindings.ImGui.ImGui.Spacing();
        Dalamud.Bindings.ImGui.ImGui.Separator();
        Dalamud.Bindings.ImGui.ImGui.Spacing();

        this.searchComponent.Draw();

        Dalamud.Bindings.ImGui.ImGui.Spacing();
        Dalamud.Bindings.ImGui.ImGui.Separator();
        Dalamud.Bindings.ImGui.ImGui.Spacing();

        this.listComponent.Draw();
    }

    public void DrawSidePanel() {
        this.editorPanelComponent.Draw();
    }

    public void Dispose() { }
}