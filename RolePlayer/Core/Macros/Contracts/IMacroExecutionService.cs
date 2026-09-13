namespace RolePlayer.Core.Macros.Contracts;

using RolePlayer.Core.Macros.Models;

public interface IMacroExecutionService {
    void Execute(RoleplayMacro macro);
}