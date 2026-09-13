namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;

public class DashboardHeaderComponent {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    public DashboardHeaderComponent(IGameDashboardPresenter presenter, ILocalizationService localization) {
        this.presenter = presenter;
        this.localization = localization;
    }

    public void Draw() {
        var isRunning = this.presenter.CurrentState != SessionState.Inactive;
        ImGui.BeginDisabled(isRunning);

        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled(this.localization.Translate("host_broadcast_channel"));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(120f);

        string broadcastChannelKey = $"channel_{this.presenter.SelectedBroadcastChannel.ToString().ToLowerInvariant()}";
        if (ImGui.BeginCombo("##BroadcastChannel", this.localization.Translate(broadcastChannelKey))) {
            foreach (GameChatChannel channel in Enum.GetValues(typeof(GameChatChannel))) {
                string channelKey = $"channel_{channel.ToString().ToLowerInvariant()}";
                if (ImGui.Selectable(this.localization.Translate(channelKey), this.presenter.SelectedBroadcastChannel == channel))
                    this.presenter.SetBroadcastChannel(channel);
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();
        ImGui.Spacing();
        ImGui.SameLine();

        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled(this.localization.Translate("host_listening_channels"));
        ImGui.SameLine();
        string channelPreview = this.presenter.SelectedChannels.Count == 0 ? this.localization.Translate("host_no_channel") : this.localization.Translate("host_channels_selected", this.presenter.SelectedChannels.Count);
        ImGui.SetNextItemWidth(150f);
        if (ImGui.BeginCombo("##ListeningChannels", channelPreview)) {
            foreach (GameChatChannel channel in Enum.GetValues(typeof(GameChatChannel))) {
                string channelKey = $"channel_{channel.ToString().ToLowerInvariant()}";
                bool isSelected = this.presenter.SelectedChannels.Contains(channel);
                if (ImGui.Checkbox(this.localization.Translate(channelKey), ref isSelected))
                    this.presenter.ToggleChannel(channel);
            }
            ImGui.EndCombo();
        }

        ImGui.EndDisabled();
    }
}