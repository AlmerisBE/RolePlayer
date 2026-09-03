namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Linq;
using System.Numerics;

public class TagsConfigSubTab {
    private ITagManagementService tagService;

    private string newTagName = string.Empty;

    private string editingTag = string.Empty;
    private string editName = string.Empty;

    public TagsConfigSubTab(ITagManagementService tagService) {
        this.tagService = tagService;
    }

    public void Draw() {
        ImGui.Text("Create New Tag");
        ImGui.SetNextItemWidth(200f);
        ImGui.InputTextWithHint("##NewTag", "Tag Name", ref this.newTagName, 32);
        ImGui.SameLine();

        if (ImGui.Button("Add Tag") && !string.IsNullOrWhiteSpace(this.newTagName)) {
            this.tagService.CreateGlobalTag(this.newTagName.Trim());
            this.newTagName = string.Empty;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var tags = this.tagService.GetAvailableTags().ToList();
        if (tags.Count == 0) {
            return;
        }

        if (ImGui.BeginTable("TagsTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn("Tag Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Emotes", ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 120f);
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
                    if (ImGui.Button($"Save##{tag}")) {
                        this.tagService.RenameGlobalTag(tag, this.editName.Trim());
                        this.editingTag = string.Empty;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Cancel##{tag}")) {
                        this.editingTag = string.Empty;
                    }
                }
                else {
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(tag);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(this.tagService.GetTagEmoteCount(tag).ToString());

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Edit##{tag}")) {
                        this.editingTag = tag;
                        this.editName = tag;
                    }
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.Button($"Delete##{tag}")) {
                        this.tagService.DeleteGlobalTag(tag);
                    }

                    ImGui.PopStyleColor();
                }
            }
            ImGui.EndTable();
        }
    }
}