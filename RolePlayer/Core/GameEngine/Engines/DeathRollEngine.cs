namespace RolePlayer.Core.GameEngine.Engines;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class DeathRollEngine : IGameEngine {
    public string EngineType => "DeathRollEngine";
    public bool IsRunning { get; private set; }

    private GameSessionConfig? config;
    private int currentMaxRoll;
    private int stageIndex;
    private List<string> defaultStages = new() { "Preparation", "Registration", "InProgress", "Finished" };

    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private readonly object stateLock = new();

    public IReadOnlyList<string> Participants {
        get {
            lock (this.stateLock) {
                return this.participants.ToList();
            }
        }
    }

    public string CurrentStageName {
        get {
            var stages = this.config?.Game?.Stages != null && this.config.Game.Stages.Count > 0 ? this.config.Game.Stages : this.defaultStages;
            return stages.Count > this.stageIndex ? stages[this.stageIndex] : "Unknown";
        }
    }

    public bool AllowChatRegistration {
        get => this.config?.Game?.AllowChatRegistration ?? false;
        set {
            lock (this.stateLock) {
                if (this.config?.Game == null || this.config.Game.AllowChatRegistration == value) return;
                this.config.Game.AllowChatRegistration = value;
            }

            if (value) this.BroadcastRequested?.Invoke(this.GetMessage("Msg_RegistrationOpened", "Registration is now open! Type !join to participate."));
            else this.BroadcastRequested?.Invoke(this.GetMessage("Msg_RegistrationClosed", "Registration is now closed."));
        }
    }

    public event Action<string>? BroadcastRequested;
    public event Action? GameFinished;
    public event Action? ParticipantsChanged;
    public event Action? StageChanged;

    public void Initialize(GameSessionConfig config) {
        this.config = config;
    }

    private string GetMessage(string key, string defaultMsg) {
        if (this.config?.Game?.Messages != null && this.config.Game.Messages.TryGetValue(key, out var customMsg)) return customMsg;
        return defaultMsg;
    }

    public void Start() {
        if (this.config == null) return;

        string startRollStr = this.config.Game?.Parameters.GetValueOrDefault("StartingRoll", "999") ?? "999";

        lock (this.stateLock) {
            if (!int.TryParse(startRollStr, out this.currentMaxRoll)) this.currentMaxRoll = 999;
            this.IsRunning = true;
            this.stageIndex = 0;
            this.participants.Clear();
        }

        this.ParticipantsChanged?.Invoke();
        this.StageChanged?.Invoke();

        this.BroadcastRequested?.Invoke(this.GetMessage("Msg_Welcome", "Welcome to Death Roll! Type !join to participate."));
    }

    public void Stop() {
        lock (this.stateLock) {
            if (!this.IsRunning) return;
            this.IsRunning = false;
            this.stageIndex = 2; // Finished
        }
        this.StageChanged?.Invoke();
        this.GameFinished?.Invoke();
    }

    public void AddParticipant(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;

        bool added = false;
        int count = 0;

        lock (this.stateLock) {
            if (this.stageIndex == 0 && this.participants.Add(name)) {
                added = true;
                count = this.participants.Count;
            }
        }

        if (added) {
            this.ParticipantsChanged?.Invoke();
            this.BroadcastRequested?.Invoke(this.GetMessage("Msg_Join", "{0} joined! ({1} ready)").Replace("{0}", name).Replace("{1}", count.ToString()));
        }
    }

    public void AdvanceStage() {
        lock (this.stateLock) {
            if (this.stageIndex == 0) {
                this.stageIndex = 1; // Passage à Registration
            }
            else if (this.stageIndex == 1) {
                if (this.participants.Count < 2) {
                    this.BroadcastRequested?.Invoke(this.GetMessage("Msg_StartWarning", "Need at least 2 players."));
                    return;
                }
                this.stageIndex = 2; // Passage à InProgress
            }
            else if (this.stageIndex == 2) {
                this.Stop();
                return;
            }
        }

        this.StageChanged?.Invoke();

        if (this.stageIndex == 1) {
            this.BroadcastRequested?.Invoke(this.GetMessage("Msg_Welcome", "Welcome to Death Roll! We are gathering players."));
        }
        else if (this.stageIndex == 2) {
            this.BroadcastRequested?.Invoke(this.GetMessage("Msg_Start", "The game begins! First to roll 1 loses."));
            this.BroadcastRequested?.Invoke(this.GetMessage("Msg_FirstToRoll", "Starting roll: 1-{0}. Someone type: /random {0}").Replace("{0}", this.currentMaxRoll.ToString()));
        }
    }

    public void ProcessEvent(GameEvent gameEvent) {
        bool running;
        int stage;
        bool allowJoin;

        lock (this.stateLock) {
            running = this.IsRunning;
            stage = this.stageIndex;
            allowJoin = this.AllowChatRegistration;
        }

        if (!running) return;

        if (stage == 1 && allowJoin && gameEvent is ChatGameEvent chatEvent) {
            string msg = chatEvent.Message.Trim().ToLowerInvariant();
            if (msg == "!join") this.AddParticipant(chatEvent.Sender);
            return;
        }

        if (stage == 2 && gameEvent is DiceRollGameEvent diceRoll) {
            lock (this.stateLock) {
                if (!this.participants.Contains(diceRoll.Sender)) return;
                if (diceRoll.OutOf != this.currentMaxRoll) return;
                this.currentMaxRoll = diceRoll.Roll;
            }

            if (this.currentMaxRoll == 1) {
                this.BroadcastRequested?.Invoke(this.GetMessage("Msg_Loss", "{0} rolled a 1 and died!").Replace("{0}", diceRoll.Sender));
                this.Stop();
            }
            else {
                this.BroadcastRequested?.Invoke(this.GetMessage("Msg_RollNext", "{0} rolled {1}. Next: /random {1}").Replace("{0}", diceRoll.Sender).Replace("{1}", this.currentMaxRoll.ToString()));
            }
        }
    }
}