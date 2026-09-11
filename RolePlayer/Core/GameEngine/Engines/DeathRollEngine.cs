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

    // Use HashSet for O(1) deduplication and case-insensitive lookups
    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private readonly object stateLock = new();
    private bool isRegistrationPhase;

    public IReadOnlyList<string> Participants {
        get {
            lock (this.stateLock) {
                return this.participants.ToList();
            }
        }
    }

    public event Action<string>? BroadcastRequested;
    public event Action? GameFinished;
    public event Action? ParticipantsChanged;

    public void Initialize(GameSessionConfig config) {
        this.config = config;
    }

    public void Start() {
        if (this.config == null) return;

        string startRollStr = this.config.Game?.Parameters.GetValueOrDefault("StartingRoll", "999") ?? "999";

        lock (this.stateLock) {
            if (!int.TryParse(startRollStr, out this.currentMaxRoll)) this.currentMaxRoll = 999;
            this.IsRunning = true;
            this.isRegistrationPhase = true;
            this.participants.Clear();
        }

        this.ParticipantsChanged?.Invoke();

        this.BroadcastRequested?.Invoke("Welcome to Death Roll! Type !join to participate.");
        this.BroadcastRequested?.Invoke("When all players are ready, type !start to begin.");
    }

    public void Stop() {
        lock (this.stateLock) {
            if (!this.IsRunning) return;
            this.IsRunning = false;
        }
        this.GameFinished?.Invoke();
    }

    public void ProcessEvent(GameEvent gameEvent) {
        bool running;
        bool regPhase;

        lock (this.stateLock) {
            running = this.IsRunning;
            regPhase = this.isRegistrationPhase;
        }

        if (!running) return;

        if (regPhase && gameEvent is ChatGameEvent chatEvent) {
            this.HandleRegistrationMessage(chatEvent);
            return;
        }

        if (!regPhase && gameEvent is DiceRollGameEvent diceRoll) {
            this.HandleDiceRoll(diceRoll);
        }
    }

    private void HandleRegistrationMessage(ChatGameEvent chat) {
        string msg = chat.Message.Trim().ToLowerInvariant();

        if (msg == "!join") {
            bool added = false;
            int count = 0;

            // Atomic operation to verify and add the participant safely
            lock (this.stateLock) {
                if (this.participants.Add(chat.Sender)) {
                    added = true;
                    count = this.participants.Count;
                }
            }

            if (added) {
                this.ParticipantsChanged?.Invoke();
                this.BroadcastRequested?.Invoke($"{chat.Sender} joined the Death Roll! ({count} players ready)");
            }
        }
        else if (msg == "!start") {
            int count = 0;

            lock (this.stateLock) {
                count = this.participants.Count;
                if (count >= 2) this.isRegistrationPhase = false;
            }

            if (count < 2) {
                this.BroadcastRequested?.Invoke("Death Roll requires at least 2 players to start. Type !join to participate.");
            }
            else {
                this.BroadcastRequested?.Invoke($"The game begins with {count} players!");
                this.BroadcastRequested?.Invoke($"First to roll 1 loses. Starting roll: 1-{this.currentMaxRoll}. To start, someone type: /random {this.currentMaxRoll}");
            }
        }
    }

    private void HandleDiceRoll(DiceRollGameEvent diceRoll) {
        lock (this.stateLock) {
            if (!this.participants.Contains(diceRoll.Sender)) return;
            if (diceRoll.OutOf != this.currentMaxRoll) return;

            this.currentMaxRoll = diceRoll.Roll;
        }

        if (this.currentMaxRoll == 1) {
            this.BroadcastRequested?.Invoke($"Player {diceRoll.Sender} rolled a 1 and died! The game has ended.");
            this.Stop();
        }
        else {
            this.BroadcastRequested?.Invoke($"Player {diceRoll.Sender} rolled {this.currentMaxRoll}. Next player, type: /random {this.currentMaxRoll}");
        }
    }
}