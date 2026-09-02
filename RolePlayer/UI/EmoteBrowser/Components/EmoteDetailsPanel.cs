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

    private string newTagInput = string.Empty;

    public EmoteDetailsPanel(
        IUnlockSourceProvider unlockSourceProvider,
        IModStateProvider modStateProvider,
        IEmoteSelectionState selectionState,
        IEmoteDebugService debugService,
        IEmoteExecutionService executionService,
        ITagManagementService tagManagementService) {

        this.unlockSourceProvider = unlockSourceProvider;
        this.modStateProvider = modStateProvider;
        this.selectionState = selectionState;
        this.debugService = debugService;
        this.executionService = executionService;
        this.tagManagementService = tagManagementService;
    }

    public void Draw() {
        var emote = this.selectionState.SelectedEmote;
        if (emote == null) {
            return;
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Name: {emote.Name}");

        // Récupération de l'icône et calcul de sa largeur avec le padding
        string closeIcon = FontAwesomeIcon.Times.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        // Alignement strict à droite
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
        this.DrawTagManagement(emote.Id);

        ImGui.Separator();
        if (ImGui.Button("Debug to Console")) {
            this.debugService.LogEmoteDetails(emote.Id);
        }
    }

    private void DrawTagManagement(uint emoteId) {
        ImGui.Text("Custom Tags:");

        var currentTags = this.tagManagementService.GetTagsForEmote(emoteId).ToList();
        foreach (var tag in currentTags) {
            ImGui.BulletText(tag);
            ImGui.SameLine();
            if (ImGui.SmallButton($"Remove##{tag}")) {
                this.tagManagementService.RemoveTagFromEmote(emoteId, tag);
            }
        }

        ImGui.SetNextItemWidth(150f);
        ImGui.InputText("##newTagInput", ref this.newTagInput, 32);
        ImGui.SameLine();

        if (ImGui.Button("Add Tag") && !string.IsNullOrWhiteSpace(this.newTagInput)) {
            this.tagManagementService.AddTagToEmote(emoteId, this.newTagInput.Trim());
            this.newTagInput = string.Empty;
        }
    }
}