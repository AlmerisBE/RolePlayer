namespace RolePlayer.Core.Macros.Engine.Services;

using Dalamud.Plugin.Services;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Models;
using System.Numerics;

public class SystemVariableProvider : ISystemVariableProvider {
    private IObjectTable objectTable;
    private ITargetManager targetManager;

    public SystemVariableProvider(IObjectTable objectTable, ITargetManager targetManager) {
        this.objectTable = objectTable;
        this.targetManager = targetManager;
    }

    public void HydrateContext(MacroExecutionContext context) {
        var player = this.objectTable.LocalPlayer;
        if (player != null) {
            context.Variables["Player.Name"] = player.Name.TextValue;
            context.Variables["Player.Level"] = player.Level;

            if (player.ClassJob.IsValid) context.Variables["Player.Job"] = player.ClassJob.Value.Abbreviation.ToString();
        }

        var target = this.targetManager.Target;
        if (target != null) {
            context.Variables["Target.Name"] = target.Name.TextValue;
            if (player != null) context.Variables["Target.Distance"] = Vector3.Distance(player.Position, target.Position);
        }
        else {
            context.Variables["Target.Name"] = string.Empty;
            context.Variables["Target.Distance"] = 0f;
        }
    }
}