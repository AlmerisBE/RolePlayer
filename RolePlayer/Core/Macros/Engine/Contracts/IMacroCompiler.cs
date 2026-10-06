namespace RolePlayer.Core.Macros.Engine.Contracts;

using RolePlayer.Core.Macros.Engine.Models;
using RolePlayer.Core.Macros.Models;

public interface IMacroCompiler {
    bool TryCompile(RoleplayMacro macro, out MacroCallFrame? frame, out string errorMessage);
}