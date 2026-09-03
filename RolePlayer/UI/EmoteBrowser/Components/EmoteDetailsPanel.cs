namespace RolePlayer.UI.EmoteBrowser.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Models;
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
    private IConfigurationService configurationService;
    private HotbarManagerComponent hotbarManager;

    public EmoteDetailsPanel(
        IUnlockSourceProvider unlockSourceProvider,
        IModStateProvider modStateProvider,
        IEmoteSelectionState selectionState,
        IEmoteDebugService debugService,
        IEmoteExecutionService executionService,
        ITagManagementService tagManagementService,
        IGroupManagementService groupManagementService,
        IConfigurationService configurationService,
        HotbarManagerComponent hotbarManager) {

        this.unlockSourceProvider = unlockSourceProvider;
        this.modStateProvider = modStateProvider;
        this.selectionState = selectionState;
        this.debugService = debugService;
        this.executionService = executionService;
        this.tagManagementService = tagManagementService;
        this.groupManagementService = groupManagementService;
        this.configurationService = configurationService;
        this.hotbarManager = hotbarManager;
    }

    public void Draw() {
        var emote = this.selectionState.SelectedEmote;
        if (emote == null) {
            return;
        }

        string closeIcon = FontAwesomeIcon.Times.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        if (ImGui.BeginTable("HeaderTable", 2)) {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("CloseBtn", ImGuiTableColumnFlags.WidthFixed, closeBtnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.SetWindowFontScale(1.3f);
            ImGui.TextUnformatted(emote.Name);
            ImGui.SetWindowFontScale(1.0f);

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{closeIcon}##CloseDetails")) {
                this.selectionState.SelectedEmote = null;
                ImGui.PopFont();
                ImGui.EndTable();
                return;
            }
            ImGui.PopFont();

            ImGui.EndTable();
        }

        ImGui.Spacing();

        if (!string.IsNullOrEmpty(emote.Category)) {
            ImGui.Text($"Category: {emote.Category}");
        }

        ImGui.Text($"Unlocked: {(emote.IsUnlocked ? "Yes" : "No")}");

        var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
        if (!string.IsNullOrEmpty(modName)) {
            ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), $"Modified by: {modName}");
        }

        ImGui.Spacing();

        if (ImGui.BeginTable("CommandsTable", 2, ImGuiTableFlags.BordersInnerH)) {
            ImGui.TableSetupColumn("Lang", ImGuiTableColumnFlags.WidthFixed, 70f);
            ImGui.TableSetupColumn("Cmd", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.TextDisabled("Command:");
            ImGui.TableNextColumn(); ImGui.TextUnformatted(emote.LocalizedCommand);

            if (!string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand != emote.LocalizedCommand) {
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.TextDisabled("English:");
                ImGui.TableNextColumn(); ImGui.TextUnformatted(emote.EnglishCommand);
            }
            ImGui.EndTable();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (emote.IsUnlocked) {
            if (ImGui.Button("Execute Emote", new Vector2(-1, 30))) {
                this.executionService.ExecuteEmote(emote.Id);
            }
        }
        else {
            ImGui.TextDisabled("You have not unlocked this emote yet.");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        this.DrawStaticHotbarAssignment(emote.Id);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        this.DrawGroupManagement(emote.Id);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        this.DrawTagManagement(emote.Id);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Debug to Console", new Vector2(-1, 0))) {
            this.debugService.LogEmoteDetails(emote.Id);
        }
    }

    private void DrawStaticHotbarAssignment(uint emoteId) {
        ImGui.TextDisabled("Static Hotbars:");

        var config = this.configurationService.GetConfig();
        var manualHotbars = config.Hotbars.Where(h => h.PopulationMode == HotbarPopulationMode.Manual).ToList();

        if (!manualHotbars.Any()) {
            ImGui.TextDisabled("No manual hotbars available.");
            return;
        }

        bool hotbarChanged = false;
        foreach (var hotbar in manualHotbars) {
            bool isInHotbar = hotbar.ManualEmoteIds.Contains(emoteId);
            if (ImGui.Checkbox($"{hotbar.Name}##hb_{hotbar.Id}", ref isInHotbar)) {
                if (isInHotbar) {
                    hotbar.ManualEmoteIds.Add(emoteId);
                }
                else {
                    hotbar.ManualEmoteIds.Remove(emoteId);
                }

                hotbarChanged = true;
            }
        }

        if (hotbarChanged) {
            this.configurationService.Save();
            this.hotbarManager.RefreshWindows();
        }
    }

    private void DrawGroupManagement(uint emoteId) {
        ImGui.TextDisabled("Group Assignment:");
        var currentGroup = this.groupManagementService.GetGroupForEmote(emoteId);
        var previewValue = string.IsNullOrEmpty(currentGroup) ? "None" : currentGroup;

        string? groupToAssign = null;
        bool removeGroup = false;

        if (ImGui.BeginTable("GroupTable", 1, ImGuiTableFlags.None)) {
            ImGui.TableSetupColumn("Combo", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.SetNextItemWidth(-1f);
            if (ImGui.BeginCombo("##GroupCombo", previewValue)) {
                if (ImGui.Selectable("None", string.IsNullOrEmpty(currentGroup))) {
                    removeGroup = true;
                }

                foreach (var group in this.groupManagementService.GetGroups()) {
                    var isSelected = group.Name == currentGroup;
                    if (ImGui.Selectable(group.Name, isSelected)) {
                        groupToAssign = group.Name;
                    }

                    if (isSelected) {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.EndTable();
        }

        if (removeGroup) {
            this.groupManagementService.RemoveEmoteFromGroup(emoteId);
        }
        else if (groupToAssign != null) {
            this.groupManagementService.AssignEmoteToGroup(emoteId, groupToAssign);
        }
    }

    private void DrawTagManagement(uint emoteId) {
        ImGui.TextDisabled("Assigned Tags:");

        var currentTags = this.tagManagementService.GetTagsForEmote(emoteId).ToList();
        string? tagToRemove = null;

        if (ImGui.BeginTable("TagsTable", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg)) {
            ImGui.TableSetupColumn("Tag", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 60f);

            if (currentTags.Count == 0) {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextDisabled("No tags assigned");
                ImGui.TableNextColumn();
            }

            foreach (var tag in currentTags) {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(tag);

                ImGui.TableNextColumn();
                if (ImGui.Button($"Remove##{tag}", new Vector2(-1f, 0))) {
                    tagToRemove = tag;
                }
            }
            ImGui.EndTable();
        }

        if (tagToRemove != null) {
            this.tagManagementService.RemoveTagFromEmote(emoteId, tagToRemove);
        }

        ImGui.Spacing();

        var availableTags = this.tagManagementService.GetAvailableTags().Except(currentTags).ToList();
        string? tagToAdd = null;

        if (ImGui.BeginTable("AddTagTable", 1, ImGuiTableFlags.None)) {
            ImGui.TableSetupColumn("Combo", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.SetNextItemWidth(-1f);
            if (availableTags.Count > 0) {
                if (ImGui.BeginCombo("##addTagCombo", "Select a tag to add...")) {
                    foreach (var tag in availableTags) {
                        if (ImGui.Selectable(tag)) {
                            tagToAdd = tag;
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            else {
                ImGui.BeginDisabled();
                if (ImGui.BeginCombo("##addTagCombo", "No available tags...")) {
                    ImGui.EndCombo();
                }

                ImGui.EndDisabled();
            }
            ImGui.EndTable();
        }

        if (tagToAdd != null) {
            this.tagManagementService.AddTagToEmote(emoteId, tagToAdd);
        }
    }
}