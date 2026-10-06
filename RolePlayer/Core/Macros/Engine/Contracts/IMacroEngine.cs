namespace RolePlayer.Core.Macros.Engine.Contracts;

using RolePlayer.Core.Macros.Models;

public interface IMacroEngine {
    bool IsRunning { get; }
    void Play(RoleplayMacro macro);
    void Stop();
}