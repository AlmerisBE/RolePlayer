namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using RolePlayer.UI.MainWindow.Contracts;
using RolePlayer.UI.MainWindow.Models;
using System.Collections.Generic;
using System.Linq;

public class AutoTranslateProvider : IAutoTranslateService {
    private IDataManager dataManager;
    private List<AutoTranslateResult> cache = new();
    private bool isLoaded = false;

    public AutoTranslateProvider(IDataManager dataManager) {
        this.dataManager = dataManager;
    }

    private void LoadCache() {
        var sheet = this.dataManager.GetExcelSheet<Completion>();
        if (sheet == null) return;

        foreach (var row in sheet) {
            var text = row.Text.ToString();
            if (string.IsNullOrWhiteSpace(text)) continue;
            if (row.Group == 0 || row.Key == 0) continue;

            this.cache.Add(new AutoTranslateResult {
                DisplayText = text,
                Payload = $"\uE040{text}\uE041"
            });
        }

        this.isLoaded = true;
    }

    public IEnumerable<AutoTranslateResult> Search(string query) {
        if (!this.isLoaded) this.LoadCache();
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2) return Enumerable.Empty<AutoTranslateResult>();

        var lowerQuery = query.ToLowerInvariant();
        return this.cache
            .Where(r => r.DisplayText.ToLowerInvariant().Contains(lowerQuery))
            .Take(30);
    }
}