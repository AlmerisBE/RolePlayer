namespace RolePlayer.Core.GameEngine.Engines;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class DeathRollEngine : IGameEngine {
    public string EngineType => "DeathRollEngine";
    public bool IsRunning { get; private set; }

    private GameSessionConfig? config;

    public event Action<string>? BroadcastRequested;
    public event Action? GameFinished;

    public void Initialize(GameSessionConfig config) {
        this.config = config;
    }

    public void Start() {
        if (this.config == null) return;
        this.IsRunning = true;

        string startRoll = this.config.Game?.Parameters.GetValueOrDefault("StartingRoll", "999") ?? "999";
        this.BroadcastRequested?.Invoke($"The Death Roll begins! First to roll 1 loses. Starting roll: 1-{startRoll}. Type /random {startRoll} to start!");
    }

    public void Stop() {
        this.IsRunning = false;
        this.GameFinished?.Invoke();
    }

    public void ProcessMessage(string sender, string message, GameChatChannel channel) {
        if (!this.IsRunning) return;

        // Logique de parsing des dés à implémenter plus tard.
    }
}