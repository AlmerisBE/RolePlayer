namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;

public interface IGameEngine {
    string EngineType { get; }
    bool IsRunning { get; }

    event Action<string>? BroadcastRequested;
    event Action? GameFinished;

    void Initialize(GameSessionConfig config);
    void Start();
    void Stop();
    void ProcessMessage(string sender, string message, GameChatChannel channel);
}