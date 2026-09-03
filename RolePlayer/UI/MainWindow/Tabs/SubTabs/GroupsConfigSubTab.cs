namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Linq;
using System.Numerics;

public class GroupsConfigSubTab {
    private IGroupManagementService groupService;

    private string newGroupName = string.Empty;
    private string newGroupDesc = string.Empty;

    private string editingGroup = string.Empty;
    private string editName = string.Empty;
    private string editDesc = string.Empty;

    public GroupsConfigSubTab(IGroupManagementService groupService) {
        this.groupService = groupService;
    }

    public void Draw() {
        ImGui.Text("Create New Group");
        ImGui.SetNextItemWidth(200f);
        ImGui.InputTextWithHint("##NewGroup", "Group Name", ref this.newGroupName, 64);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(300f);
        ImGui.InputTextWithHint("##NewDesc", "Description", ref this.newGroupDesc, 256);
        ImGui.SameLine();

        if (ImGui.Button("Add Group") && !string.IsNullOrWhiteSpace(this.newGroupName)) {
            this.groupService.CreateGroup(new EmoteGroup { Name = this.newGroupName.Trim(), Description = this.newGroupDesc.Trim() });
            this.newGroupName = string.Empty;
            this.newGroupDesc = string.Empty;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var groups = this.groupService.GetGroups().ToList();
        if (groups.Count == 0) {
            return;
        }

        if (ImGui.BeginTable("GroupsTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 200f);
            ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Emotes", ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 120f);
            ImGui.TableHeadersRow();

            foreach (var group in groups) {
                ImGui.TableNextRow();

                if (this.editingGroup == group.Name) {
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    ImGui.InputText($"##EditName_{group.Name}", ref this.editName, 64);

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    ImGui.InputText($"##EditDesc_{group.Name}", ref this.editDesc, 256);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(this.groupService.GetGroupEmoteCount(group.Name).ToString());

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Save##{group.Name}")) {
                        this.groupService.UpdateGroup(group.Name, this.editName.Trim(), this.editDesc.Trim());
                        this.editingGroup = string.Empty;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Cancel##{group.Name}")) {
                        this.editingGroup = string.Empty;
                    }
                }
                else {
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(group.Name);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(group.Description);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(this.groupService.GetGroupEmoteCount(group.Name).ToString());

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Edit##{group.Name}")) {
                        this.editingGroup = group.Name;
                        this.editName = group.Name;
                        this.editDesc = group.Description;
                    }
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.Button($"Delete##{group.Name}")) {
                        this.groupService.DeleteGroup(group.Name);
                    }

                    ImGui.PopStyleColor();
                }
            }
            ImGui.EndTable();
        }
    }
}