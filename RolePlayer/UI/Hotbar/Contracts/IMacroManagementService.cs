namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.Core.Macros.Models;
using System;
using System.Collections.Generic;

public interface IMacroManagementService {
    IEnumerable<RoleplayMacro> GetMacros();
    void CreateMacro(RoleplayMacro macro);
    void UpdateMacro(Guid id, string name, string content, uint iconId);
    void DeleteMacro(Guid id);
}