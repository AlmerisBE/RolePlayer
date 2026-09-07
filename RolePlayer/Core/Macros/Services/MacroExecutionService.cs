namespace RolePlayer.Core.Macros.Services;

using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.Hotbar.Contracts;
using System;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureMacroModule;

public class MacroExecutionService : IMacroExecutionService {
    public unsafe void Execute(RoleplayMacro macro) {
        if (macro == null || string.IsNullOrWhiteSpace(macro.Content)) return;

        var raptureShellModule = RaptureShellModule.Instance();
        if (raptureShellModule == null) {
            return;
        }

        var lines = macro.Content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int lineCount = Math.Min(lines.Length, 15); // Les macros natives sont limitées à 15 lignes

        Macro tempMacro = new RaptureMacroModule.Macro();
        tempMacro.Name.Ctor();

        for (int i = 0; i < lineCount; i++) {
            tempMacro.Lines[i].Ctor();
            tempMacro.Lines[i].SetString(lines[i]);
        }

        raptureShellModule->ExecuteMacro(&tempMacro);

        // Nettoyage impératif de la mémoire native allouée pour les chaînes de caractères
        for (int i = 0; i < lineCount; i++) tempMacro.Lines[i].Dtor();
        tempMacro.Name.Dtor();
    }
}