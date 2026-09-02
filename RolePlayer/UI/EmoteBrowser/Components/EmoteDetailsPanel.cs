namespace RolePlayer.UI.EmoteBrowser.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Numerics;

public class EmoteDetailsPanel {
    private IUnlockSourceProvider unlockSourceProvider;
    private IModStateProvider modStateProvider;
    private IEmoteSelectionState selectionState;
    private IEmoteDebugService debugService;
    private IEmoteExecutionService executionService;

    public EmoteDetailsPanel(
        IUnlockSourceProvider unlockSourceProvider,
        IModStateProvider modStateProvider,
        IEmoteSelectionState selectionState,
        IEmoteDebugService debugService,
        IEmoteExecutionService executionService) {

        this.unlockSourceProvider = unlockSourceProvider;
        this.modStateProvider = modStateProvider;
        this.selectionState = selectionState;
        this.debugService = debugService;
        this.executionService = executionService;
    }

    public void Draw() {
        var emote = this.selectionState.SelectedEmote;
        if (emote == null) {
            ImGui.TextDisabled("Select an emote to view details.");
            return;
        }

        ImGui.Text($"Name: {emote.Name}");
        ImGui.Text($"Unlocked: {(emote.IsUnlocked ? "Yes" : "No")}");

        var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
        if (!string.IsNullOrEmpty(modName)) {
            ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), $"Modified by: {modName}");
        }

        var unlockSource = this.unlockSourceProvider.GetUnlockSource(emote.Id);
        ImGui.TextWrapped($"Source: {unlockSource}");

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

        if (ImGui.Button("Debug to Console")) {
            this.debugService.LogEmoteDetails(emote.Id);
        }
    }
}