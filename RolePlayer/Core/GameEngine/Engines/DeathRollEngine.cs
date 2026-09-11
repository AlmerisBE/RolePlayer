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

        this.BroadcastRequested?.Invoke("Welcome to Death Roll! The first to roll a 1 loses.");
        this.BroadcastRequested?.Invoke($"To start, one player must type: /random {this.currentMaxRoll}");
    }

    public void Stop() {
        if (!this.IsRunning) return;
        this.IsRunning = false;
        this.GameFinished?.Invoke();
    }

    public void ProcessEvent(GameEvent gameEvent) {
        if (!this.IsRunning) return;

        if (gameEvent is DiceRollGameEvent diceRoll) {
            this.HandleDiceRoll(diceRoll);
        }
    }

    private void HandleDiceRoll(DiceRollGameEvent diceRoll) {
        if (diceRoll.OutOf != this.currentMaxRoll) return;

        this.currentMaxRoll = diceRoll.Roll;

        if (this.currentMaxRoll == 1) {
            this.BroadcastRequested?.Invoke($"Player {diceRoll.Sender} rolled a 1 and died! The game has ended.");
            this.Stop();
        }
        else {
            this.BroadcastRequested?.Invoke($"Player {diceRoll.Sender} rolled {this.currentMaxRoll}. Next player, it's your turn to type: /random {this.currentMaxRoll}");
        }
    }
}