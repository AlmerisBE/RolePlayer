namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class MainWindow : Window, IDisposable {
    private IEnumerable<IEmoteBrowserTab> tabs;
    private EmoteDetailsPanel detailsPanel;
    private IEmoteSelectionState selectionState;
    private IClientState clientState;

    private const float BaseWidth = 400f;
    private const float SidePanelWidth = 300f;
    private bool lastPanelState = false;
    private bool isFirstFrame = true;

    public MainWindow(
        IEnumerable<IEmoteBrowserTab> tabs,
        EmoteDetailsPanel detailsPanel,
        IEmoteSelectionState selectionState,
        IClientState clientState)
        : base("RolePlayer", ImGuiWindowFlags.None) {

        this.tabs = tabs.OrderBy(t => t.SortOrder).ToList();

        this.detailsPanel = detailsPanel;
        this.selectionState = selectionState;
        this.clientState = clientState;

        this.clientState.Logout += this.OnLogout;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(BaseWidth, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        var isPanelOpen = this.selectionState.SelectedEmote != null;

        // Auto-correction de la largeur lors du premier rendu suite à un rechargement du plugin
        if (this.isFirstFrame) {
            var initialSize = ImGui.GetWindowSize();
            if (!isPanelOpen && initialSize.X >= BaseWidth + SidePanelWidth - 20f) {
                ImGui.SetWindowSize(new Vector2(initialSize.X - SidePanelWidth, initialSize.Y));
            }

            this.isFirstFrame = false;
            this.lastPanelState = isPanelOpen;
        }
        else if (isPanelOpen != this.lastPanelState) {
            var currentSize = ImGui.GetWindowSize();
            var targetWidth = isPanelOpen ? currentSize.X + SidePanelWidth : currentSize.X - SidePanelWidth;
            if (targetWidth < BaseWidth) {
                targetWidth = BaseWidth;
            }

            ImGui.SetWindowSize(new Vector2(targetWidth, currentSize.Y));
            this.lastPanelState = isPanelOpen;
        }

        var contentWidth = isPanelOpen ? -(SidePanelWidth + ImGui.GetStyle().ItemSpacing.X) : 0;

        if (ImGui.BeginChild("MainContent", new Vector2(contentWidth, 0), false)) {
            this.DrawTabs();
        }
        ImGui.EndChild();

        if (isPanelOpen) {
            ImGui.SameLine();
            if (ImGui.BeginChild("SidePanel", new Vector2(SidePanelWidth, 0), true)) {
                this.detailsPanel.Draw();
            }
            ImGui.EndChild();
        }
    }

    private void DrawTabs() {
        if (ImGui.BeginTabBar("MainTabBar", ImGuiTabBarFlags.Reorderable)) {
            foreach (var tab in this.tabs) {
                if (ImGui.BeginTabItem(tab.TabName)) {
                    tab.Draw();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    private void OnLogout(int type, int code) {
        this.selectionState.SelectedEmote = null;
    }

    public void Dispose() {
        this.clientState.Logout -= this.OnLogout;
    }
}