namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameEventWatcher : IDisposable {
    event Action<GameEvent>? EventFired;

    void Start();
    void Stop();
    void SetParticipants(IEnumerable<string> participantNames);
    void ClearParticipants();
}