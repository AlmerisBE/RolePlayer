namespace RolePlayer.UI.MainWindow.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public class StatusBarComponent : IDisposable {
    private IEmoteExecutionService executionService;
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IClientState clientState;

    private int unlockedEmotesCount = 0;
    private int totalEmotesCount = 0;

    public StatusBarComponent(
        IEmoteExecutionService executionService,
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IClientState clientState) {

        this.executionService = executionService;
        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.clientState = clientState;

        this.clientState.Login += this.OnLogin;
        this.CalculateEmoteStatsAsync();
    }

    private void CalculateEmoteStatsAsync() {
        Task.Run(() => {
            var emotes = this.emoteRepository.GetBaseEmotes().ToList();
            this.totalEmotesCount = emotes.Count;
            this.unlockedEmotesCount = emotes.Count(e => !e.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(e.Id));
        });
    }

    public void Draw() {
        if (ImGui.BeginChild("StatusBar", new Vector2(0, 0), false, ImGuiWindowFlags.NoScrollbar)) {
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.UserFriends.ToIconString())) {
                this.executionService.OpenNativeEmoteWindow();
            }

            ImGui.PopFont();

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Ouvrir la fenêtre des emotes du jeu");
            }

            var statsText = $"{this.unlockedEmotesCount} / {this.totalEmotesCount} débloquées";
            var textSize = ImGui.CalcTextSize(statsText).X + 30f;

            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - textSize);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(statsText);
        }
        ImGui.EndChild();
    }

    private void OnLogin() => this.CalculateEmoteStatsAsync();

    public void Dispose() => this.clientState.Login -= this.OnLogin;
}