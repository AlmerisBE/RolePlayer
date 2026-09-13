namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using RolePlayer.UI.MainWindow.Components;
using RolePlayer.UI.MainWindow.Tabs;
using System;
using System.Numerics;

public class MainWindow : Window, IDisposable {
    private TabManagerComponent tabManager;
    private StatusBarComponent statusBar;
    private MainLayoutComponent layoutManager;

    public MainWindow(
        IDalamudPluginInterface pluginInterface,
        TabManagerComponent tabManager,
        StatusBarComponent statusBar,
        MainLayoutComponent layoutManager)
        : base($"RolePlayer v{pluginInterface.Manifest.AssemblyVersion}", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {

        this.tabManager = tabManager;
        this.statusBar = statusBar;
        this.layoutManager = layoutManager;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(400f, 400f),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void OpenConfig() {
        this.IsOpen = true;
        this.tabManager.RequestTab<ConfigurationTab>();
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