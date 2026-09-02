namespace RolePlayer.UI.Command.Services;

using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using RolePlayer.UI.Command.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class CommandDispatcher : IDisposable {
    private ICommandManager commandManager;
    private IEnumerable<ICommand> commands;
    private string mainCommand = "/roleplayer";

    public CommandDispatcher(ICommandManager commandManager, IEnumerable<ICommand> commands) {
        this.commandManager = commandManager;
        this.commands = commands;

        this.commandManager.AddHandler(this.mainCommand, new CommandInfo(this.OnCommand) {
            HelpMessage = "Type '/roleplayer help' for more information."
        });
    }

    private void OnCommand(string command, string arguments) {
        var args = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        // Si aucun argument n'est fourni, on cible "emotes" par défaut
        var subCommand = args.Length > 0 ? args[0].ToLowerInvariant() : "emotes";
        var subArguments = args.Length > 1 ? args[1] : string.Empty;

        var targetCommand = this.commands.FirstOrDefault(c => c.CommandTrigger.Equals(subCommand, StringComparison.InvariantCultureIgnoreCase));

        if (targetCommand != null) {
            targetCommand.Execute(subArguments);
        }
    }

    public void Dispose() {
        this.commandManager.RemoveHandler(this.mainCommand);
    }
}