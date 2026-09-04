namespace RolePlayer.UI.MainWindow.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
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
    private IContextManagementService contextService;

    private int unlockedEmotesCount = 0;
    private int totalEmotesCount = 0;

    public StatusBarComponent(
        IEmoteExecutionService executionService,
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IClientState clientState,
        IContextManagementService contextService) {

        this.executionService = executionService;
        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.clientState = clientState;
        this.contextService = contextService;

        this.playerStateProvider.PlayerStateValid += this.CalculateEmoteStatsAsync;
        this.CalculateEmoteStatsAsync();
    }

    private void CalculateEmoteStatsAsync() {
        if (!this.playerStateProvider.IsPlayerValid) {
            return;
        }

        Task.Run(() => {
            var emotes = this.emoteRepository.GetBaseEmotes().ToList();
            this.totalEmotesCount = emotes.Count;
            this.unlockedEmotesCount = emotes.Count(e => !e.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(e.Id));
        });
    }

    public void Draw() {
        var height = ImGui.GetFrameHeight();

        if (ImGui.BeginChild("StatusBar", new Vector2(0, height), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)) {
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.UserFriends.ToIconString())) {
                this.executionService.OpenNativeEmoteWindow();
            }

            ImGui.PopFont();

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Open native Emote window");
            }

            ImGui.SameLine();

            var currentContext = this.contextService.GetCurrentContext();
            ImGui.SetNextItemWidth(150f);
            if (ImGui.BeginCombo("##QuickContextSwitch", currentContext.Name)) {
                foreach (var ctx in this.contextService.GetAllContexts()) {
                    if (ImGui.Selectable(ctx.Name, ctx.Id == currentContext.Id)) {
                        this.contextService.SwitchContext(ctx.Id);
                    }
                }
                ImGui.EndCombo();
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Switch Active Context");
            }

            var statsText = $"{this.unlockedEmotesCount} / {this.totalEmotesCount} unlocked";
            var textSize = ImGui.CalcTextSize(statsText).X + ImGui.GetStyle().WindowPadding.X;

            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - textSize);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(statsText);
        }
        ImGui.EndChild();
    }

    public void Dispose() => this.playerStateProvider.PlayerStateValid -= this.CalculateEmoteStatsAsync;
}