namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System.Linq;
using System.Numerics;

public class TagsConfigSubTab {
    private ITagManagementService tagService;
    private ILocalizationService localization;

    private string newTagName = string.Empty;

    private string editingTag = string.Empty;
    private string editName = string.Empty;

    private string tagToDelete = string.Empty;
    private bool isDeleteDialogOpen = false;

    public TagsConfigSubTab(ITagManagementService tagService, ILocalizationService localization) {
        this.tagService = tagService;
        this.localization = localization;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_tag_create"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float btnWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(availableWidth - btnWidth - spacing);
        ImGui.InputTextWithHint("##NewTag", this.localization.Translate("config_tag_name_hint"), ref this.newTagName, 32);
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddTag", new Vector2(btnWidth, 0)) && !string.IsNullOrWhiteSpace(this.newTagName)) {
            this.tagService.CreateGlobalTag(this.newTagName.Trim());
            this.newTagName = string.Empty;
        }
        ImGui.PopFont();
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_tag_tooltip_add"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var tags = this.tagService.GetAvailableTags().ToList();
        if (tags.Count == 0) return;

        if (ImGui.BeginTable("TagsTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_tag_col_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_hb_table_emotes"), ImGuiTableColumnFlags.WidthFixed, 50f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 75f);
            ImGui.TableHeadersRow();

            foreach (var tag in tags) {
                ImGui.TableNextRow();

                if (this.editingTag == tag) {
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    ImGui.InputText($"##EditTag_{tag}", ref this.editName, 32);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(this.tagService.GetTagEmoteCount(tag).ToString());

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Save.ToIconString()}##Save_{tag}")) {
                        this.tagService.RenameGlobalTag(tag, this.editName.Trim());
                        this.editingTag = string.Empty;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"{FontAwesomeIcon.Times.ToIconString()}##Cancel_{tag}")) this.editingTag = string.Empty;

                    ImGui.PopFont();
                }
                else {
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(tag);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(this.tagService.GetTagEmoteCount(tag).ToString());

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Edit.ToIconString()}##Edit_{tag}")) {
                        this.editingTag = tag;
                        this.editName = tag;
                    }
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##Del_{tag}")) {
                        this.tagToDelete = tag;
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
            ImGui.OpenPopup(this.localization.Translate("config_tag_del_title"));
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal(this.localization.Translate("config_tag_del_title"), ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            ImGui.Text(this.localization.Translate("config_tag_del_desc", this.tagToDelete));
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), this.localization.Translate("config_tag_del_warn"));
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.localization.Translate("config_common_yes_delete"), new Vector2(120, 0))) {
                this.tagService.DeleteGlobalTag(this.tagToDelete);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
}