namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Numerics;

public class MainWindow : Window, IDisposable {
    private IEnumerable<IEmoteBrowserTab> tabs;
    private EmoteDetailsPanel detailsPanel;
    private IEmoteSelectionState selectionState;
    private IClientState clientState;

    private const float BaseWidth = 350f;
    private const float SidePanelWidth = 300f;
    private bool lastPanelState = false;

    public MainWindow(
        IEnumerable<IEmoteBrowserTab> tabs,
        EmoteDetailsPanel detailsPanel,
        IEmoteSelectionState selectionState,
        IClientState clientState)
        : base("RolePlayer", ImGuiWindowFlags.None) {

        this.tabs = tabs;
        this.detailsPanel = detailsPanel;
        this.selectionState = selectionState;
        this.clientState = clientState;

        this.clientState.Logout += this.OnLogout;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(BaseWidth, 350),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        var isPanelOpen = this.selectionState.SelectedEmote != null;

        if (isPanelOpen != this.lastPanelState) {
            var currentSize = ImGui.GetWindowSize();
            var targetWidth = isPanelOpen ? currentSize.X + SidePanelWidth : currentSize.X - SidePanelWidth;
            if (targetWidth < BaseWidth) {
                targetWidth = BaseWidth;
            }

            ImGui.SetWindowSize(new Vector2(targetWidth, currentSize.Y));
            this.lastPanelState = isPanelOpen;
        }

        if (isPanelOpen) {
            if (ImGui.BeginTable("MainWindowLayout", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV)) {
                ImGui.TableSetupColumn("MainContent", ImGuiTableColumnFlags.WidthStretch, 0.65f);
                ImGui.TableSetupColumn("SidePanel", ImGuiTableColumnFlags.WidthFixed, SidePanelWidth);
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                this.DrawTabs();

                ImGui.TableNextColumn();
                this.detailsPanel.Draw();

                ImGui.EndTable();
            }
        }
        else {
            this.DrawTabs();
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

    // Restauration de la signature exacte attendue par l'API Dalamud
    private void OnLogout(int type, int code) {
        this.selectionState.SelectedEmote = null;
    }

    public void Dispose() {
        this.clientState.Logout -= this.OnLogout;
    }
}