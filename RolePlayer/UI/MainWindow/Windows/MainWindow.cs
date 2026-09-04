namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.UI.MainWindow.Components;
using System;
using System.Numerics;

public class MainWindow : Window, IDisposable {
    private TabManagerComponent tabManager;
    private StatusBarComponent statusBar;
    private MainLayoutComponent layoutManager;
    private IClientState clientState;

    public MainWindow(
        IDalamudPluginInterface pluginInterface,
        TabManagerComponent tabManager,
        StatusBarComponent statusBar,
        MainLayoutComponent layoutManager,
        IClientState clientState)
        : base($"RolePlayer v{pluginInterface.Manifest.AssemblyVersion}", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {

        this.tabManager = tabManager;
        this.statusBar = statusBar;
        this.layoutManager = layoutManager;
        this.clientState = clientState;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(400f, 400f),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        bool isPanelOpen = this.tabManager.ActiveTab?.IsSidePanelOpen ?? false;

        this.layoutManager.Draw(
            drawMainContent: () => this.tabManager.Draw(),
            drawSidePanel: () => this.tabManager.ActiveTab?.DrawSidePanel(),
            isPanelOpen: isPanelOpen
        );

        ImGui.Separator();
        this.statusBar.Draw();
    }

    public void Dispose() {
        this.statusBar.Dispose();
    }
}