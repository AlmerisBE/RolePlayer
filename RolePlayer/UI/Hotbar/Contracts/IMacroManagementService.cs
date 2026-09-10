namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.Core.Macros.Models;
using System;
using System.Collections.Generic;

public interface IMacroManagementService {
    IEnumerable<RoleplayMacro> GetMacros();
    void CreateMacro(RoleplayMacro macro);
    void UpdateMacro(Guid id, string name, string content, uint iconId, bool isLocked);
    void DeleteMacro(Guid id);
    void AppendToMacro(Guid id, string command);
}