namespace RolePlayer.UI.GameDashboard.Presenters;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using System.Collections.Generic;
using System.Linq;

public class GameDashboardPresenter : IGameDashboardPresenter {
    private IGameLibraryService libraryService;
    private IGameSessionService sessionService;

    public IReadOnlyList<GameDefinition> AvailableGames => this.libraryService.GetAvailableGames().ToList();
    public IReadOnlyList<string> Participants => this.sessionService.Participants;
    public GameDefinition? SelectedGame { get; private set; }
    public HashSet<GameChatChannel> SelectedChannels { get; private set; } = new();
    public SessionState CurrentState => this.sessionService.CurrentState;

    public GameDashboardPresenter(IGameLibraryService libraryService, IGameSessionService sessionService) {
        this.libraryService = libraryService;
        this.sessionService = sessionService;
    }

    public void SelectGame(GameDefinition? game) {
        if (this.CurrentState != SessionState.Inactive) return;
        this.SelectedGame = game;
    }

    public void ToggleChannel(GameChatChannel channel) {
        if (this.CurrentState != SessionState.Inactive) return;

        if (!this.SelectedChannels.Add(channel)) this.SelectedChannels.Remove(channel);
    }

    public void StartSession() {
        if (this.SelectedGame == null || !this.SelectedChannels.Any()) return;

        var config = new GameSessionConfig {
            Game = this.SelectedGame,
            ListeningChannels = new HashSet<GameChatChannel>(this.SelectedChannels)
        };

        this.sessionService.StartSession(config);
    }

    public void StopSession() {
        this.sessionService.StopSession();
    }

    public void Dispose() {
        this.sessionService.StopSession();
    }
}