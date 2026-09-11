namespace RolePlayer.UI.GameDashboard.Presenters;

using Dalamud.Plugin.Services;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using System.Collections.Generic;
using System.Linq;

public class GameDashboardPresenter : IGameDashboardPresenter {
    private IGameLibraryService libraryService;
    private IGameSessionService sessionService;
    private ITargetManager targetManager;

    public IReadOnlyList<GameDefinition> AvailableGames => this.libraryService.GetAvailableGames().ToList();
    public GameDefinition? SelectedGame { get; private set; }
    public HashSet<GameChatChannel> SelectedChannels { get; private set; } = new();
    public SessionState CurrentState => this.sessionService.CurrentState;
    public IReadOnlyList<string> Participants => this.sessionService.Participants;

    public string CurrentStageName => this.sessionService.CurrentStageName;

    public bool AllowChatRegistration {
        get => this.sessionService.AllowChatRegistration;
        set => this.sessionService.AllowChatRegistration = value;
    }

    public string CurrentTargetName => this.targetManager.Target?.Name.TextValue ?? string.Empty;

    public GameDashboardPresenter(IGameLibraryService libraryService, IGameSessionService sessionService, ITargetManager targetManager) {
        this.libraryService = libraryService;
        this.sessionService = sessionService;
        this.targetManager = targetManager;
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

    public void AdvanceStage() {
        this.sessionService.AdvanceStage();
    }

    public void AddTarget() {
        var targetName = this.CurrentTargetName;
        if (!string.IsNullOrWhiteSpace(targetName)) this.sessionService.AddParticipant(targetName);
    }

    public void RemoveParticipant(string name) {
        if (!string.IsNullOrWhiteSpace(name)) this.sessionService.RemoveParticipant(name);
    }

    public void SaveGameConfig() {
        if (this.SelectedGame != null) this.libraryService.SaveGame(this.SelectedGame);
    }

    public void Dispose() {
        this.sessionService.StopSession();
    }
}