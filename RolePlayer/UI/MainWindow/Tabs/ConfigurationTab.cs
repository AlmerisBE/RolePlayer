namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Linq;

public class ConfigurationTab : IEmoteBrowserTab {
    private IGroupManagementService groupService;
    private ITagManagementService tagService;

    private string newGroupName = string.Empty;
    private string newGroupDesc = string.Empty;
    private int newGroupSort = 0;

    private string newGlobalTagInput = string.Empty;

    public string TabName => "Configuration";
    public int SortOrder => 100; // Toujours en dernier
    public bool SupportsSidePanel => false;

    public ConfigurationTab(IGroupManagementService groupService, ITagManagementService tagService) {
        this.groupService = groupService;
        this.tagService = tagService;
    }

    public void Draw() {
        if (ImGui.BeginTabBar("ConfigTabBar")) {
            if (ImGui.BeginTabItem("Groups")) {
                this.DrawGroupsTab();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Tags")) {
                this.DrawTagsTab();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }

    private void DrawGroupsTab() {
        ImGui.Spacing();
        ImGui.Text("Add New Group");
        ImGui.InputText("Name##newGroup", ref this.newGroupName, 64);
        ImGui.InputText("Description##newGroup", ref this.newGroupDesc, 128);
        ImGui.InputInt("Sort Order##newGroup", ref this.newGroupSort);

        if (ImGui.Button("Create Group") && !string.IsNullOrWhiteSpace(this.newGroupName)) {
            this.groupService.CreateGroup(new EmoteGroup {
                Name = this.newGroupName.Trim(),
                Description = this.newGroupDesc.Trim(),
                SortOrder = this.newGroupSort
            });
            this.newGroupName = string.Empty;
            this.newGroupDesc = string.Empty;
            this.newGroupSort = 0;
        }

        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Text("Existing Groups");

        var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;
        if (ImGui.BeginTable("GroupsTable", 4, tableFlags)) {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch, 0.6f);
            ImGui.TableSetupColumn("Sort", ImGuiTableColumnFlags.WidthFixed, 50f);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableHeadersRow();

            foreach (var group in this.groupService.GetGroups().ToList()) {
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text(group.Name);
                ImGui.TableNextColumn(); ImGui.Text(group.Description);
                ImGui.TableNextColumn(); ImGui.Text(group.SortOrder.ToString());

                ImGui.TableNextColumn();
                if (ImGui.Button($"Delete##{group.Name}")) {
                    this.groupService.DeleteGroup(group.Name);
                }
            }
            ImGui.EndTable();
        }
    }

    private void DrawTagsTab() {
        ImGui.Spacing();
        ImGui.Text("Create New Tag");
        ImGui.SetNextItemWidth(200f);
        ImGui.InputText("##newGlobalTag", ref this.newGlobalTagInput, 32);
        ImGui.SameLine();

        if (ImGui.Button("Create Tag") && !string.IsNullOrWhiteSpace(this.newGlobalTagInput)) {
            this.tagService.CreateGlobalTag(this.newGlobalTagInput);
            this.newGlobalTagInput = string.Empty;
        }

        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Text("Available Tags");

        var tags = this.tagService.GetAvailableTags().ToList();
        if (tags.Count == 0) {
            ImGui.TextDisabled("No tags have been created yet.");
        }

        foreach (var tag in tags) {
            ImGui.BulletText(tag);
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - 60f);
            if (ImGui.Button($"Delete##{tag}")) {
                this.tagService.DeleteGlobalTag(tag);
            }
        }
    }
}