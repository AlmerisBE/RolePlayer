namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Collections.Generic;
using System.Numerics;

public class MainWindow : Window {
    private IEnumerable<IEmoteBrowserTab> tabs;
    private EmoteDetailsPanel detailsPanel;

    public MainWindow(IEnumerable<IEmoteBrowserTab> tabs, EmoteDetailsPanel detailsPanel)
        : base("RolePlayer - Emotes", ImGuiWindowFlags.None) {
        this.tabs = tabs;
        this.detailsPanel = detailsPanel;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        // Utilisation d'une table ImGui pour séparer le contenu principal (gauche) du panneau de détails (droite)
        if (ImGui.BeginTable("MainWindowLayout", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV)) {
            ImGui.TableSetupColumn("MainContent", ImGuiTableColumnFlags.WidthStretch, 0.7f);
            ImGui.TableSetupColumn("SidePanel", ImGuiTableColumnFlags.WidthStretch, 0.3f);
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            this.DrawTabs();

            ImGui.TableNextColumn();
            this.detailsPanel.Draw();

            ImGui.EndTable();
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
}