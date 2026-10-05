namespace RolePlayer.API.GameData.Commands;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.Command.Contracts;
using System;
using System.Text.RegularExpressions;

public class LogMessageCommand : ICommand {
    private IDataManager dataManager;
    private ILoggerService logger;

    public string CommandTrigger => "logmessage";
    public string Description => "Dumps Excel sheet data to the plugin log. Usage: /roleplayer logmessage [search]";

    public LogMessageCommand(IDataManager dataManager, ILoggerService logger) {
        this.dataManager = dataManager;
        this.logger = logger;
    }

    public void Execute(string keyword) {
        var sheet = this.dataManager.GetExcelSheet<LogMessage>();
        if (sheet == null) return;

        this.logger.Info($"--- Starting LogMessage scan for keyword: '{keyword}' ---");

        foreach (var row in sheet) {
            string rawText = row.Text.ToString();
            if (string.IsNullOrWhiteSpace(rawText)) continue;

            if (rawText.Contains(keyword, StringComparison.OrdinalIgnoreCase)) {
                string cleanText = Regex.Replace(rawText, @"[\x00-\x1F]+", "|");
                this.logger.Info($"[ID: {row.RowId}] {cleanText}");
            }
        }

        this.logger.Info("--- Scan complete ---");
    }
}