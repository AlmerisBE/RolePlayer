namespace RolePlayer.UI.GameDashboard.Contracts;

using RolePlayer.Core.Emotes.Models;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameDashboardPresenter : IDisposable {
    IReadOnlyList<GameDefinition> AvailableGames { get; }
    IReadOnlyList<EnrichedEmote> EmotesCache { get; }
    GameDefinition? SelectedGame { get; }
    HashSet<GameChatChannel> SelectedChannels { get; }
    SessionState CurrentState { get; }
    IReadOnlyList<string> Participants { get; }
    IReadOnlyDictionary<string, object> SessionVariables { get; }

    string CurrentStageName { get; }
    bool AllowChatRegistration { get; set; }
    string CurrentTargetName { get; }

    void SelectGame(GameDefinition? game);
    void ToggleChannel(GameChatChannel channel);
    void StartSession();
    void StopSession();

    void AdvanceStage();
    void AddTarget();
    void RemoveParticipant(string name);
    void SetSessionVariable(string key, object value);

    void SaveGameConfig();
}