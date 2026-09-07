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
        // En assumant que les macros soient stockées au niveau du profil personnage
        return this.configService.GetCurrentProfile().Macros;
    }

    public void CreateMacro(RoleplayMacro macro) {
        if (macro == null || string.IsNullOrWhiteSpace(macro.Name)) return;

        this.configService.GetCurrentProfile().Macros.Add(macro);
        this.configService.Save();
    }

    public void UpdateMacro(Guid id, string name, string content, uint iconId) {
        var macro = this.configService.GetCurrentProfile().Macros.FirstOrDefault(m => m.Id == id);
        if (macro == null) return;

        macro.Name = name;
        macro.Content = content;
        macro.IconId = iconId;
        this.configService.Save();
    }

    public void DeleteMacro(Guid id) {
        var profile = this.configService.GetCurrentProfile();
        var removed = profile.Macros.RemoveAll(m => m.Id == id);

        if (removed > 0) this.configService.Save();
    }
}