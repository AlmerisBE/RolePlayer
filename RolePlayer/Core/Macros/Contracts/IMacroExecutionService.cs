namespace RolePlayer.Core.Macros.Contracts;

using RolePlayer.Core.Macros.Models;

public interface IMacroExecutionService {
    bool IsExecuting { get; }
    void Execute(RoleplayMacro macro);
    void CancelAll();
}