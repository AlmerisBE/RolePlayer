namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System.Linq;
using System.Numerics;

public class GroupsConfigSubTab {
    private IGroupManagementService groupService;
    private ILocalizationService localization;

    private string newGroupName = string.Empty;
    private string newGroupDesc = string.Empty;

    private string editingGroup = string.Empty;
    private string editName = string.Empty;
    private string editDesc = string.Empty;

    private string groupToDelete = string.Empty;
    private bool isDeleteDialogOpen = false;

    public GroupsConfigSubTab(IGroupManagementService groupService, ILocalizationService localization) {
        this.groupService = groupService;
        this.localization = localization;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_grp_create"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float remainingWidth = availableWidth - buttonWidth - (spacing * 2);

        float nameWidth = remainingWidth * 0.35f;
        float descWidth = remainingWidth * 0.65f;

        ImGui.SetNextItemWidth(nameWidth);
        ImGui.InputTextWithHint("##NewGroup", this.localization.Translate("config_grp_name_hint"), ref this.newGroupName, 64);
        ImGui.SameLine();

        ImGui.SetNextItemWidth(descWidth);
        ImGui.InputTextWithHint("##NewDesc", this.localization.Translate("config_grp_desc_hint"), ref this.newGroupDesc, 256);
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddGroup", new Vector2(buttonWidth, 0)) && !string.IsNullOrWhiteSpace(this.newGroupName)) {
            this.groupService.CreateGroup(new EmoteGroup { Name = this.newGroupName.Trim(), Description = this.newGroupDesc.Trim() });
            this.newGroupName = string.Empty;
            this.newGroupDesc = string.Empty;
        }
        ImGui.PopFont();
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_grp_tooltip_add"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var groups = this.groupService.GetGroups().ToList();
        if (groups.Count == 0) return;

        if (ImGui.BeginTable("GroupsTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn(this.localization.Translate("config_grp_col_desc"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_hb_table_emotes"), ImGuiTableColumnFlags.WidthFixed, 50f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 75f);
            ImGui.TableHeadersRow();

            foreach (var group in groups) {
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 28f);

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
                    if (ImGui.Button($"{FontAwesomeIcon.Times.ToIconString()}##Cancel_{group.Name}")) this.editingGroup = string.Empty;

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
            ImGui.OpenPopup(this.localization.Translate("config_grp_del_title"));
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal(this.localization.Translate("config_grp_del_title"), ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            ImGui.Text(this.localization.Translate("config_grp_del_desc", this.groupToDelete));
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), this.localization.Translate("config_grp_del_warn"));
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.localization.Translate("config_common_yes_delete"), new Vector2(120, 0))) {
                this.groupService.DeleteGroup(this.groupToDelete);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
}