namespace RolePlayer.Core.Macros.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.Hotbar.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class MacroManagementService : IMacroManagementService {
    private IConfigurationService configService;

    public MacroManagementService(IConfigurationService configService) {
        this.configService = configService;
    }

    public IEnumerable<RoleplayMacro> GetMacros() {
        return this.configService.GetCurrentProfile().Macros;
    }

    public void CreateMacro(RoleplayMacro macro) {
        if (macro == null || string.IsNullOrWhiteSpace(macro.Name)) return;

        this.configService.GetCurrentProfile().Macros.Add(macro);
        this.configService.Save();
    }

    public void UpdateMacro(Guid id, string name, string content, uint iconId, bool isLocked) {
        var macro = this.configService.GetCurrentProfile().Macros.FirstOrDefault(m => m.Id == id);
        if (macro == null) return;

        macro.Name = name;
        macro.Content = content;
        macro.IconId = iconId;
        macro.IsLocked = isLocked;
        this.configService.Save();
    }

    public void DeleteMacro(Guid id) {
        var profile = this.configService.GetCurrentProfile();
        if (profile.Macros.RemoveAll(m => m.Id == id) > 0) this.configService.Save();
    }

    public void AppendToMacro(Guid id, string command) {
        if (string.IsNullOrWhiteSpace(command)) return;

        var macro = this.configService.GetCurrentProfile().Macros.FirstOrDefault(m => m.Id == id);
        if (macro == null || macro.IsLocked) return;

        string prefix = string.IsNullOrEmpty(macro.Content) ? string.Empty : "\n";
        macro.Content += $"{prefix}{command.Trim()}";

        this.configService.Save();
    }
}