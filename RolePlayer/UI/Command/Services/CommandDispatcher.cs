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
    private string aliasCommand = "/rp";

    public CommandDispatcher(ICommandManager commandManager, IEnumerable<ICommand> commands) {
        this.commandManager = commandManager;
        this.commands = commands;

        var commandInfo = new CommandInfo(this.OnCommand) {
            HelpMessage = "Type '/roleplayer help' or '/rp help' for more information."
        };

        this.commandManager.AddHandler(this.mainCommand, commandInfo);
        this.commandManager.AddHandler(this.aliasCommand, commandInfo);
    }

    private void OnCommand(string command, string arguments) {
        var args = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        var subCommand = args.Length > 0 ? args[0].ToLowerInvariant() : "emotes";
        var subArguments = args.Length > 1 ? args[1] : string.Empty;

        var targetCommand = this.commands.FirstOrDefault(c => c.CommandTrigger.Equals(subCommand, StringComparison.InvariantCultureIgnoreCase));

        if (targetCommand != null) {
            targetCommand.Execute(subArguments);
        }
    }

    public void Dispose() {
        this.commandManager.RemoveHandler(this.mainCommand);
        this.commandManager.RemoveHandler(this.aliasCommand);
    }
}