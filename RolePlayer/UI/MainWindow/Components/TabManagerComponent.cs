namespace RolePlayer.UI.MainWindow.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Collections.Generic;
using System.Linq;

public class TabManagerComponent {
    private IEnumerable<IEmoteBrowserTab> tabs;

    public IEmoteBrowserTab? ActiveTab { get; private set; }

    public TabManagerComponent(IEnumerable<IEmoteBrowserTab> tabs) {
        this.tabs = tabs.OrderBy(t => t.SortOrder).ToList();
    }

    public void Draw() {
        if (ImGui.BeginTabBar("MainTabBar", ImGuiTabBarFlags.Reorderable)) {
            foreach (var tab in this.tabs) {
                if (ImGui.BeginTabItem(tab.TabName)) {
                    this.ActiveTab = tab;
                    tab.Draw();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
}