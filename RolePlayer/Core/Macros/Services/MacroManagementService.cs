namespace RolePlayer.Core.Macros.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.Core.Macros.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class MacroManagementService : IMacroManagementService, IDisposable {
    private IConfigurationService configService;

    public MacroManagementService(IConfigurationService configService) {
        this.configService = configService;
        this.configService.ProfileLoaded += this.EnsureCommandIdsAreAssigned;
        this.EnsureCommandIdsAreAssigned();
    }

    public void EnsureCommandIdsAreAssigned() {
        var profile = this.configService.GetCurrentProfile();
        if (profile == null) return;

        bool changed = false;
        var allMacros = profile.Macros;

        int nextId = allMacros.Any() ? allMacros.Max(m => m.CommandId) + 1 : 1;

        foreach (var macro in allMacros) {
            if (macro.CommandId == 0) {
                macro.CommandId = nextId++;
                changed = true;
            }
        }

        if (changed) this.configService.Save();
    }

    public RoleplayMacro? GetMacroByCommandId(int commandId) {
        return this.GetMacros().FirstOrDefault(m => m.CommandId == commandId);
    }

    public IEnumerable<RoleplayMacro> GetMacros() {
        return this.configService.GetCurrentProfile().Macros;
    }

    public void CreateMacro(RoleplayMacro macro) {
        if (macro == null || string.IsNullOrWhiteSpace(macro.Name)) return;

        var macros = this.configService.GetCurrentProfile().Macros;
        macro.CommandId = macros.Any() ? macros.Max(m => m.CommandId) + 1 : 1;

        macros.Add(macro);
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
        var macro = this.configService.GetCurrentProfile().Macros.FirstOrDefault(m => m.Id == id);

        if (macro == null || macro.IsLocked) return;

        if (string.IsNullOrEmpty(macro.Content)) macro.Content = command;
        else macro.Content += $"\n{command}";

        this.configService.Save();
    }

    public void Dispose() {
        this.configService.ProfileLoaded -= this.EnsureCommandIdsAreAssigned;
    }
}