namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.MainWindow.Components;
using System;
using System.Numerics;

public class MainWindow : Window, IDisposable {
    private TabManagerComponent tabManager;
    private StatusBarComponent statusBar;
    private MainLayoutComponent layoutManager;
    private EmoteDetailsPanel detailsPanel;
    private IEmoteSelectionState selectionState;
    private IClientState clientState;

    public MainWindow(
        IDalamudPluginInterface pluginInterface,
        TabManagerComponent tabManager,
        StatusBarComponent statusBar,
        MainLayoutComponent layoutManager,
        EmoteDetailsPanel detailsPanel,
        IEmoteSelectionState selectionState,
        IClientState clientState)
        : base($"RolePlayer v{pluginInterface.Manifest.AssemblyVersion}", ImGuiWindowFlags.None) {

        this.tabManager = tabManager;
        this.statusBar = statusBar;
        this.layoutManager = layoutManager;
        this.detailsPanel = detailsPanel;
        this.selectionState = selectionState;
        this.clientState = clientState;

        this.clientState.Logout += this.OnLogout;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(400f, 400f),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        this.layoutManager.Draw(
            drawMainContent: () => this.tabManager.Draw(),
            drawSidePanel: () => this.detailsPanel.Draw(),
            isPanelOpen: this.selectionState.SelectedEmote != null
        );

        Dalamud.Bindings.ImGui.ImGui.Separator();
        this.statusBar.Draw();
    }

    private void OnLogout(int type, int code) => this.selectionState.SelectedEmote = null;

    public void Dispose() {
        this.clientState.Logout -= this.OnLogout;
        this.statusBar.Dispose();
    }
}