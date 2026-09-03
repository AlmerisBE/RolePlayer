namespace RolePlayer.Core.Configuration.Commands;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.Command.Contracts;

public class ContextCommand : ICommand {
    private IContextManagementService contextService;

    public string CommandTrigger => "context";
    public string Description => "Switch active context. Usage: /roleplayer context [name]";

    public ContextCommand(IContextManagementService contextService) {
        this.contextService = contextService;
    }

    public void Execute(string arguments) {
        if (!string.IsNullOrWhiteSpace(arguments)) {
            this.contextService.SwitchContextByName(arguments.Trim());
        }
    }
}