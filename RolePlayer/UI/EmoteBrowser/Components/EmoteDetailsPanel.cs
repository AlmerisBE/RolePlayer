namespace RolePlayer.UI.EmoteBrowser.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Linq;
using System.Numerics;

public class EmoteDetailsPanel {
    private IUnlockSourceProvider unlockSourceProvider;
    private IModStateProvider modStateProvider;
    private IEmoteSelectionState selectionState;
    private IEmoteDebugService debugService;
    private IEmoteExecutionService executionService;
    private ITagManagementService tagManagementService;
    private IGroupManagementService groupManagementService;

    public EmoteDetailsPanel(
        IUnlockSourceProvider unlockSourceProvider,
        IModStateProvider modStateProvider,
        IEmoteSelectionState selectionState,
        IEmoteDebugService debugService,
        IEmoteExecutionService executionService,
        ITagManagementService tagManagementService,
        IGroupManagementService groupManagementService) {

        this.unlockSourceProvider = unlockSourceProvider;
        this.modStateProvider = modStateProvider;
        this.selectionState = selectionState;
        this.debugService = debugService;
        this.executionService = executionService;
        this.tagManagementService = tagManagementService;
        this.groupManagementService = groupManagementService;
    }

    public void Draw() {
        var emote = this.selectionState.SelectedEmote;
        if (emote == null) {
            return;
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Name: {emote.Name}");

        string closeIcon = FontAwesomeIcon.Times.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        var alignX = ImGui.GetWindowContentRegionMax().X - closeBtnWidth;
        if (alignX > ImGui.GetCursorPosX()) {
            ImGui.SameLine(alignX);
        }

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{closeIcon}##CloseDetails")) {
            this.selectionState.SelectedEmote = null;
            ImGui.PopFont();
            return;
        }
        ImGui.PopFont();

        if (!string.IsNullOrEmpty(emote.Category)) {
            ImGui.Text($"Category: {emote.Category}");
        }

        ImGui.Text($"Unlocked: {(emote.IsUnlocked ? "Yes" : "No")}");

        var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
        if (!string.IsNullOrEmpty(modName)) {
            ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), $"Modified by: {modName}");
        }

        ImGui.TextWrapped($"Source: {emote.UnlockRequirement}");

        ImGui.Separator();

        if (emote.IsUnlocked) {
            if (ImGui.Button("Execute Emote", new Vector2(-1, 30))) {
                this.executionService.ExecuteEmote(emote.Id);
            }
        }
        else {
            ImGui.TextDisabled("You have not unlocked this emote yet.");
        }

        ImGui.Separator();
        this.DrawGroupManagement(emote.Id);

        ImGui.Separator();
        this.DrawTagManagement(emote.Id);

        ImGui.Separator();
        if (ImGui.Button("Debug to Console")) {
            this.debugService.LogEmoteDetails(emote.Id);
        }
    }

    private void DrawGroupManagement(uint emoteId) {
        var currentGroup = this.groupManagementService.GetGroupForEmote(emoteId);
        var previewValue = string.IsNullOrEmpty(currentGroup) ? "None" : currentGroup;

        if (ImGui.BeginCombo("Group", previewValue)) {
            if (ImGui.Selectable("None", string.IsNullOrEmpty(currentGroup))) {
                this.groupManagementService.RemoveEmoteFromGroup(emoteId);
            }

            foreach (var group in this.groupManagementService.GetGroups()) {
                var isSelected = group.Name == currentGroup;
                if (ImGui.Selectable(group.Name, isSelected)) {
                    this.groupManagementService.AssignEmoteToGroup(emoteId, group.Name);
                }

                if (isSelected) {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }

    private void DrawTagManagement(uint emoteId) {
        ImGui.Text("Assigned Tags:");

        var currentTags = this.tagManagementService.GetTagsForEmote(emoteId).ToList();
        string? tagToRemove = null;

        foreach (var tag in currentTags) {
            ImGui.BulletText(tag);
            ImGui.SameLine();
            if (ImGui.SmallButton($"Remove##{tag}")) {
                tagToRemove = tag;
            }
        }

        if (tagToRemove != null) {
            this.tagManagementService.RemoveTagFromEmote(emoteId, tagToRemove);
        }

        // On détermine les tags qui n'ont pas encore été assignés à l'emote
        var availableTags = this.tagManagementService.GetAvailableTags().Except(currentTags).ToList();

        if (availableTags.Count > 0) {
            ImGui.SetNextItemWidth(150f);
            if (ImGui.BeginCombo("##addTagCombo", "Select a tag...")) {
                foreach (var tag in availableTags) {
                    if (ImGui.Selectable(tag)) {
                        this.tagManagementService.AddTagToEmote(emoteId, tag);
                    }
                }
                ImGui.EndCombo();
            }
        }
        else {
            ImGui.TextDisabled("No more tags available.");
        }
    }
}