namespace RolePlayer.UI.GameDashboard.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameDashboardPresenter : IDisposable {
    IReadOnlyList<GameDefinition> AvailableGames { get; }
    GameDefinition? SelectedGame { get; }
    HashSet<GameChatChannel> SelectedChannels { get; }
    SessionState CurrentState { get; }

    void SelectGame(GameDefinition? game);
    void ToggleChannel(GameChatChannel channel);
    void StartSession();
    void StopSession();
}