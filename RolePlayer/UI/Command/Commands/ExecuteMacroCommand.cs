namespace RolePlayer.UI.Command.Commands;

using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Command.Contracts;

public class ExecuteMacroCommand : ICommand {
    private IMacroManagementService macroService;
    private IMacroExecutionService executionService;

    public string CommandTrigger => "macro";
    public string Description => "Executes a custom macro by its ID. Usage: /rp macro <ID>";

    public ExecuteMacroCommand(IMacroManagementService macroService, IMacroExecutionService executionService) {
        this.macroService = macroService;
        this.executionService = executionService;
    }

    public void Execute(string arguments) {
        if (string.IsNullOrWhiteSpace(arguments)) return;

        if (int.TryParse(arguments.Trim(), out int macroId)) {
            var macro = this.macroService.GetMacroByCommandId(macroId);
            if (macro != null) {
                this.executionService.Execute(macro);
            }
        }
    }
}