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
                ImGui.SetTooltip("Open native Emote window");
            }

            var statsText = $"{this.unlockedEmotesCount} / {this.totalEmotesCount} unlocked";

            var currentContext = this.contextService.GetCurrentContext();
            var totalWidth = ImGui.CalcTextSize(statsText).X + 30f + 150f + ImGui.GetStyle().ItemSpacing.X;

            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - totalWidth);

            ImGui.SetNextItemWidth(150f);
            if (ImGui.BeginCombo("##QuickContextSwitch", currentContext.Name)) {
                foreach (var ctx in this.contextService.GetAllContexts()) {
                    if (ImGui.Selectable(ctx.Name, ctx.Id == currentContext.Id)) {
                        this.contextService.SwitchContext(ctx.Id);
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(statsText);
        }
        ImGui.EndChild();
    }

    private void OnLogin() => this.CalculateEmoteStatsAsync();

    public void Dispose() => this.clientState.Login -= this.OnLogin;
}