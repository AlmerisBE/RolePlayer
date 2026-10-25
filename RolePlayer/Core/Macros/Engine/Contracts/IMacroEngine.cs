namespace RolePlayer.Core.Macros.Engine.Contracts;

using RolePlayer.Core.Macros.Models;
using System;

public interface IMacroEngine {
    bool IsRunning { get; }
    Guid? ExecutingMacroId { get; }
    void Play(RoleplayMacro macro);
    void Stop();
}