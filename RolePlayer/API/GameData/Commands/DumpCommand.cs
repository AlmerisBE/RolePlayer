namespace RolePlayer.API.GameData.Commands;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.Command.Contracts;

public class DumpCommand : ICommand {
    private IDataManager dataManager;
    private ILoggerService logger;

    public string CommandTrigger => "dump";
    public string Description => "Dumps Excel sheet data to the plugin log. Usage: /roleplayer dump [search]";

    public DumpCommand(IDataManager dataManager, ILoggerService logger) {
        this.dataManager = dataManager;
        this.logger = logger;
    }

    public void Execute(string arguments) {
        var search = arguments.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(search)) {
            search = "emote";
        }

        this.logger.Info($"--- DUMPING MainCommand SHEET FOR: '{search}' ---");

        var sheet = this.dataManager.GetExcelSheet<MainCommand>();
        if (sheet == null) {
            return;
        }

        foreach (var row in sheet) {
            var name = row.Name.ToString();
            var description = row.Description.ToString();

            if (name.ToLowerInvariant().Contains(search) || description.ToLowerInvariant().Contains(search)) {
                this.logger.Info($"MainCommand ID: {row.RowId} | Name: {name} | Description: {description.Replace("\n", " ")}");
            }
        }

        this.logger.Info("--- DUMP COMPLETE ---");
    }
}