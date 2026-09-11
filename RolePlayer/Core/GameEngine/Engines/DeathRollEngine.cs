namespace RolePlayer.Core.GameEngine.Engines;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class DeathRollEngine : IGameEngine {
    public string EngineType => "DeathRollEngine";
    public bool IsRunning { get; private set; }

    private GameSessionConfig? config;
    private int currentMaxRoll;

    private List<string> participants = new();
    private bool isRegistrationPhase;

    public event Action<string>? BroadcastRequested;
    public event Action? GameFinished;

    public void Initialize(GameSessionConfig config) {
        this.config = config;
    }

    public void Start() {
        if (this.config == null) return;

        string startRollStr = this.config.Game?.Parameters.GetValueOrDefault("StartingRoll", "999") ?? "999";
        if (!int.TryParse(startRollStr, out this.currentMaxRoll)) this.currentMaxRoll = 999;

        this.IsRunning = true;
        this.isRegistrationPhase = true;
        this.participants.Clear();

        this.BroadcastRequested?.Invoke("Welcome to Death Roll! Type !join to participate.");
        this.BroadcastRequested?.Invoke("When all players are ready, type !start to begin.");
    }

    public void Stop() {
        if (!this.IsRunning) return;
        this.IsRunning = false;
        this.GameFinished?.Invoke();
    }

    public void ProcessEvent(GameEvent gameEvent) {
        if (!this.IsRunning) return;

        if (this.isRegistrationPhase && gameEvent is ChatGameEvent chatEvent) {
            this.HandleRegistrationMessage(chatEvent);
            return;
        }

        if (!this.isRegistrationPhase && gameEvent is DiceRollGameEvent diceRoll) {
            this.HandleDiceRoll(diceRoll);
        }
    }

    private void HandleRegistrationMessage(ChatGameEvent chat) {
        string msg = chat.Message.Trim().ToLowerInvariant();

        if (msg == "!join") {
            if (!this.participants.Contains(chat.Sender)) {
                this.participants.Add(chat.Sender);
                this.BroadcastRequested?.Invoke($"{chat.Sender} joined the Death Roll! ({this.participants.Count} players ready)");
            }
        }
        else if (msg == "!start") {
            if (this.participants.Count < 2) {
                this.BroadcastRequested?.Invoke("Death Roll requires at least 2 players to start. Type !join to participate.");
            }
            else {
                this.isRegistrationPhase = false;
                this.BroadcastRequested?.Invoke($"The game begins with {this.participants.Count} players!");
                this.BroadcastRequested?.Invoke($"First to roll 1 loses. Starting roll: 1-{this.currentMaxRoll}. To start, someone type: /random {this.currentMaxRoll}");
            }
        }
    }

    private void HandleDiceRoll(DiceRollGameEvent diceRoll) {
        if (!this.participants.Contains(diceRoll.Sender)) return;

        if (diceRoll.OutOf != this.currentMaxRoll) return;

        this.currentMaxRoll = diceRoll.Roll;

        if (this.currentMaxRoll == 1) {
            this.BroadcastRequested?.Invoke($"Player {diceRoll.Sender} rolled a 1 and died! The game has ended.");
            this.Stop();
        }
        else {
            this.BroadcastRequested?.Invoke($"Player {diceRoll.Sender} rolled {this.currentMaxRoll}. Next player, type: /random {this.currentMaxRoll}");
        }
    }
}