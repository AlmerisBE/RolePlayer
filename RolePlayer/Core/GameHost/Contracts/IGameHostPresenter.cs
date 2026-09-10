namespace RolePlayer.UI.GameHost.Contracts;

using RolePlayer.Core.GameHost.Models;
using System;
using System.Collections.Generic;

public interface IGameHostPresenter : IDisposable {
    IReadOnlyList<GameDefinition> AvailableGames { get; }
    GameDefinition? SelectedGame { get; }
    HashSet<GameChatChannel> SelectedChannels { get; }
    SessionState CurrentState { get; }

    void SelectGame(GameDefinition? game);
    void ToggleChannel(GameChatChannel channel);
    void StartSession();
    void StopSession();
}