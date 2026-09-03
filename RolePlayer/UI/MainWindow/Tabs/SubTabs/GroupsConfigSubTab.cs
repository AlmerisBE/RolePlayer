namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
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

    private string groupToDelete = string.Empty;
    private bool isDeleteDialogOpen = false;

    public GroupsConfigSubTab(IGroupManagementService groupService) {
        this.groupService = groupService;
    }

    public void Draw() {
        ImGui.Text("Create New Group");

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float remainingWidth = availableWidth - buttonWidth - (spacing * 2);

        float nameWidth = remainingWidth * 0.35f;
        float descWidth = remainingWidth * 0.65f;

        ImGui.SetNextItemWidth(nameWidth);
        ImGui.InputTextWithHint("##NewGroup", "Group Name", ref this.newGroupName, 64);
        ImGui.SameLine();

        ImGui.SetNextItemWidth(descWidth);
        ImGui.InputTextWithHint("##NewDesc", "Description", ref this.newGroupDesc, 256);
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddGroup", new Vector2(buttonWidth, 0)) && !string.IsNullOrWhiteSpace(this.newGroupName)) {
            this.groupService.CreateGroup(new EmoteGroup { Name = this.newGroupName.Trim(), Description = this.newGroupDesc.Trim() });
            this.newGroupName = string.Empty;
            this.newGroupDesc = string.Empty;
        }
        ImGui.PopFont();
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Add Group");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var groups = this.groupService.GetGroups().ToList();
        if (groups.Count == 0) {
            return;
        }

        if (ImGui.BeginTable("GroupsTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Emotes", ImGuiTableColumnFlags.WidthFixed, 50f);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 75f);
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
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Save.ToIconString()}##Save_{group.Name}")) {
                        this.groupService.UpdateGroup(group.Name, this.editName.Trim(), this.editDesc.Trim());
                        this.editingGroup = string.Empty;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"{FontAwesomeIcon.Times.ToIconString()}##Cancel_{group.Name}")) {
                        this.editingGroup = string.Empty;
                    }

                    ImGui.PopFont();
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
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Edit.ToIconString()}##Edit_{group.Name}")) {
                        this.editingGroup = group.Name;
                        this.editName = group.Name;
                        this.editDesc = group.Description;
                    }
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##Del_{group.Name}")) {
                        this.groupToDelete = group.Name;
                        this.isDeleteDialogOpen = true;
                    }
                    ImGui.PopStyleColor();
                    ImGui.PopFont();
                }
            }
            ImGui.EndTable();
        }

        this.DrawDeleteConfirmationModal();
    }

    private void DrawDeleteConfirmationModal() {
        if (this.isDeleteDialogOpen) {
            ImGui.OpenPopup("Delete Group Confirmation");
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal("Delete Group Confirmation", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            ImGui.Text($"Are you sure you want to delete the group '{this.groupToDelete}'?");
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), "This will remove the group assignment from all associated emotes.");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Yes, Delete", new Vector2(120, 0))) {
                this.groupService.DeleteGroup(this.groupToDelete);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0))) {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
}