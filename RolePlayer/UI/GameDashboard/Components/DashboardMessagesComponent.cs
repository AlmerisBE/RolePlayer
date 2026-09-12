namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.UI.GameDashboard.Contracts;
using System.Linq;
using System.Numerics;

public class DashboardMessagesComponent {
    private IGameDashboardPresenter presenter;

    public DashboardMessagesComponent(IGameDashboardPresenter presenter) {
        this.presenter = presenter;
    }

    public void Draw() {
        if (this.presenter.SelectedGame == null) return;

        if (ImGui.CollapsingHeader("Messages du jeu (Édition)")) {
            bool changed = false;

            float availableWidth = ImGui.GetContentRegionAvail().X;
            float btnWidth = 32f;

            ImGui.SetCursorPosX(availableWidth - btnWidth);
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddMsg", new Vector2(btnWidth, 0))) {
                this.presenter.SelectedGame.Messages[$"New_Message_{this.presenter.SelectedGame.Messages.Count + 1}"] = string.Empty;
                changed = true;
            }
            ImGui.PopFont();

            if (this.presenter.SelectedGame.Messages.Count == 0) {
                ImGui.TextDisabled("Aucun message défini.");
                return;
            }

            if (ImGui.BeginTable("MessagesTable", 3, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp)) {
                ImGui.TableSetupColumn("Clé", ImGuiTableColumnFlags.WidthStretch, 0.35f);
                ImGui.TableSetupColumn("Message", ImGuiTableColumnFlags.WidthStretch, 0.65f);
                ImGui.TableSetupColumn("Act", ImGuiTableColumnFlags.WidthFixed, 30f);
                ImGui.TableHeadersRow();

                string? keyToRemove = null;
                var keys = this.presenter.SelectedGame.Messages.Keys.ToList();

                for (int i = 0; i < keys.Count; i++) {
                    var key = keys[i];
                    var val = this.presenter.SelectedGame.Messages[key];

                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    string newKey = key;
                    ImGui.SetNextItemWidth(-1f);
                    if (ImGui.InputText($"##MsgKey_{i}", ref newKey, 64)) {
                        if (newKey != key && !string.IsNullOrWhiteSpace(newKey) && !this.presenter.SelectedGame.Messages.ContainsKey(newKey)) {
                            this.presenter.SelectedGame.Messages.Remove(key);
                            this.presenter.SelectedGame.Messages[newKey] = val;
                            changed = true;
                        }
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    if (ImGui.InputText($"##MsgVal_{i}", ref val, 256)) {
                        this.presenter.SelectedGame.Messages[keys[i]] = val;
                        changed = true;
                    }

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##DelMsg_{i}")) keyToRemove = keys[i];
                    ImGui.PopStyleColor();
                    ImGui.PopFont();
                }
                ImGui.EndTable();

                if (keyToRemove != null) {
                    this.presenter.SelectedGame.Messages.Remove(keyToRemove);
                    changed = true;
                }
            }

            if (changed) this.presenter.SaveGameConfig();
        }
    }
}