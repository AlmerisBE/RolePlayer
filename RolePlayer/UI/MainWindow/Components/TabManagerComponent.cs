namespace RolePlayer.UI.MainWindow.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class TabManagerComponent {
    private IEnumerable<IEmoteBrowserTab> tabs;
    private Type? requestedTabType = null;

    public IEmoteBrowserTab? ActiveTab { get; private set; }

    public TabManagerComponent(IEnumerable<IEmoteBrowserTab> tabs) {
        this.tabs = tabs.OrderBy(t => t.SortOrder).ToList();
    }

    public void RequestTab<T>() where T : IEmoteBrowserTab {
        this.requestedTabType = typeof(T);
    }

    public void Draw() {
        if (ImGui.BeginTabBar("MainTabBar", ImGuiTabBarFlags.None)) {
            foreach (var tab in this.tabs) {
                var flags = ImGuiTabItemFlags.None;
                if (this.requestedTabType == tab.GetType()) {
                    flags |= ImGuiTabItemFlags.SetSelected;
                    this.requestedTabType = null;
                }

                if (ImGui.BeginTabItem(tab.TabName, flags)) {
                    this.ActiveTab = tab;
                    tab.Draw();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
}