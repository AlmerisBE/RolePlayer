namespace RolePlayer.UI.MainWindow.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class StatusBarComponent : IDisposable {
    private IEmoteExecutionService executionService;
    private IEmoteCache emoteCache;
    private IContextManagementService contextService;
    private ILocalizationService localization;

    private int unlockedEmotesCount = 0;
    private int totalEmotesCount = 0;

    public StatusBarComponent(
        IEmoteExecutionService executionService,
        IEmoteCache emoteCache,
        IContextManagementService contextService,
        ILocalizationService localization) {

        this.executionService = executionService;
        this.emoteCache = emoteCache;
        this.contextService = contextService;
        this.localization = localization;

        this.emoteCache.CacheUpdated += this.UpdateStats;
        this.UpdateStats();
    }

    private void UpdateStats() {
        if (!this.emoteCache.IsReady) return;

        var emotes = this.emoteCache.GetCachedEmotes();
        this.totalEmotesCount = emotes.Count;
        this.unlockedEmotesCount = emotes.Count(e => e.IsUnlocked);
    }

    public void Draw() {
        var height = ImGui.GetFrameHeight();

        if (ImGui.BeginChild("StatusBar", new Vector2(0, height), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)) {
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.UserFriends.ToIconString())) this.executionService.OpenNativeEmoteWindow();
            ImGui.PopFont();

            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("main_status_tooltip_emote"));

            ImGui.SameLine();

            var currentContext = this.contextService.GetCurrentContext();
            ImGui.SetNextItemWidth(150f);
            if (ImGui.BeginCombo("##QuickContextSwitch", currentContext.Name)) {
                foreach (var ctx in this.contextService.GetAllContexts()) {
                    if (ImGui.Selectable(ctx.Name, ctx.Id == currentContext.Id)) this.contextService.SwitchContext(ctx.Id);
                }
                ImGui.EndCombo();
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("main_status_tooltip_context"));

            var statsText = this.localization.Translate("main_status_unlocked", this.unlockedEmotesCount, this.totalEmotesCount);
            var textSize = ImGui.CalcTextSize(statsText).X + ImGui.GetStyle().WindowPadding.X;

            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - textSize);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(statsText);
        }
        ImGui.EndChild();
    }

    public void Dispose() {
        this.emoteCache.CacheUpdated -= this.UpdateStats;
    }
}