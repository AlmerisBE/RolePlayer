namespace RolePlayer.Core.Macros.Engine.Contracts;

using RolePlayer.Core.Macros.Engine.Models;

public interface ISystemVariableProvider {
    void HydrateContext(MacroExecutionContext context);
}