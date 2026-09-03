namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.MetaData.Models;
using System;
using System.Linq;

public class TagsGroupsConfigSubTab {
    private IConfigurationService configService;
    private string newTagName = string.Empty;
    private string newGroupName = string.Empty;

    public TagsGroupsConfigSubTab(IConfigurationService configService) {
        this.configService = configService;
    }

    public void Draw() {
        var config = this.configService.GetConfig();
        bool configChanged = false;

        ImGui.Text("Tag Management");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.SetNextItemWidth(200f);
        ImGui.InputTextWithHint("##NewTag", "New tag name...", ref this.newTagName, 64);
        ImGui.SameLine();

        if (ImGui.Button("Add Tag") && !string.IsNullOrWhiteSpace(this.newTagName)) {
            if (config.AvailableTags.Add(this.newTagName.Trim())) {
                configChanged = true;
            }

            this.newTagName = string.Empty;
        }

        ImGui.Spacing();

        foreach (var tag in config.AvailableTags.ToList()) {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(tag);
            ImGui.SameLine(250f);

            if (ImGui.Button($"Remove##Tag_{tag}")) {
                config.AvailableTags.Remove(tag);
                configChanged = true;
            }
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Group Management");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.SetNextItemWidth(200f);
        ImGui.InputTextWithHint("##NewGroup", "New group name...", ref this.newGroupName, 64);
        ImGui.SameLine();

        if (ImGui.Button("Add Group") && !string.IsNullOrWhiteSpace(this.newGroupName)) {
            var groupName = this.newGroupName.Trim();
            if (!config.EmoteGroups.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))) {
                config.EmoteGroups.Add(new EmoteGroup { Name = groupName });
                configChanged = true;
            }
            this.newGroupName = string.Empty;
        }

        ImGui.Spacing();

        foreach (var group in config.EmoteGroups.ToList()) {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(group.Name);
            ImGui.SameLine(250f);

            if (ImGui.Button($"Remove##Group_{group.Name}")) {
                config.EmoteGroups.Remove(group);
                configChanged = true;
            }
        }

        if (configChanged) {
            this.configService.Save();
        }
    }
}