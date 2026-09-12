namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.GameDashboard.Contracts;
using System.Linq;

public class DashboardMessagesComponent {
    private IGameDashboardPresenter presenter;
    private readonly string[] defaultMessageKeys = { "Msg_RegistrationOpened", "Msg_RegistrationClosed", "Msg_Join", "Msg_Welcome", "Msg_StartWarning", "Msg_Start", "Msg_FirstToRoll", "Msg_Loss", "Msg_RollNext" };

    public DashboardMessagesComponent(IGameDashboardPresenter presenter) {
        this.presenter = presenter;
    }

    public void Draw() {
        if (this.presenter.SelectedGame == null) return;

        if (ImGui.CollapsingHeader("Messages du jeu (Édition)")) {
            bool messagesChanged = false;

            foreach (var key in this.defaultMessageKeys) {
                if (!this.presenter.SelectedGame.Messages.ContainsKey(key)) this.presenter.SelectedGame.Messages[key] = string.Empty;
            }

            foreach (var key in this.presenter.SelectedGame.Messages.Keys.ToList()) {
                ImGui.TextDisabled(key);
                string val = this.presenter.SelectedGame.Messages[key];
                ImGui.SetNextItemWidth(-1f);
                if (ImGui.InputText($"##msg_{key}", ref val, 256)) {
                    this.presenter.SelectedGame.Messages[key] = val;
                    messagesChanged = true;
                }
                ImGui.Spacing();
            }

            if (messagesChanged) this.presenter.SaveGameConfig();
        }
    }
}