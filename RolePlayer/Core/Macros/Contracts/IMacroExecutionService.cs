namespace RolePlayer.Core.Macros.Contracts;

using RolePlayer.Core.Macros.Models;
using System;

public interface IMacroExecutionService {
    bool IsExecuting { get; }
    Guid? ExecutingMacroId { get; }
    void Execute(RoleplayMacro macro);
    void CancelAll();
}