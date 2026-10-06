namespace RolePlayer.Core.Macros.Contracts;

using RolePlayer.Core.Macros.Models;
using System;
using System.Collections.Generic;

public interface IMacroManagementService {
    IEnumerable<RoleplayMacro> GetMacros();
    RoleplayMacro? GetMacroByCommandId(int commandId);
    void EnsureCommandIdsAreAssigned();
    void CreateMacro(RoleplayMacro macro);
    void UpdateMacro(Guid id, string name, string content, uint iconId, bool isLocked);
    void DeleteMacro(Guid id);
    void AppendToMacro(Guid id, string command);
}