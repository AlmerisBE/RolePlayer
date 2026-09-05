namespace RolePlayer.API.FFXIVCollect.Providers;

using Dalamud.Game;
using Dalamud.Plugin.Services;
using RolePlayer.API.FFXIVCollect.Models;
using RolePlayer.API.GameData.Providers;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class FFXIVCollectUnlockSourceProvider : IUnlockSourceProvider, IDisposable {
    private LuminaUnlockSourceProvider fallbackProvider;
    private IClientState clientState;
    private ILoggerService logger;
    private HttpClient httpClient;

    private Dictionary<uint, string> externalCache;
    private bool isReady;

    public FFXIVCollectUnlockSourceProvider(LuminaUnlockSourceProvider fallbackProvider, IClientState clientState, ILoggerService logger) {
        this.fallbackProvider = fallbackProvider;
        this.clientState = clientState;
        this.logger = logger;
        this.httpClient = new HttpClient();
        this.externalCache = new Dictionary<uint, string>();
        this.isReady = false;

        string langCode = this.clientState.ClientLanguage switch {
            ClientLanguage.French => "fr",
            ClientLanguage.German => "de",
            ClientLanguage.Japanese => "ja",
            _ => "en"
        };

        Task.Run(() => this.FetchExternalDataAsync(langCode));
    }

    private async Task FetchExternalDataAsync(string language) {
        try {
            var response = await this.httpClient.GetAsync($"https://ffxivcollect.com/api/emotes?language={language}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<FFXIVCollectResponse>(json);

            if (data?.Results == null) return;

            foreach (var emote in data.Results) {
                if (emote.Sources == null || emote.Sources.Count == 0) continue;

                var sourcesText = string.Join(" / ", emote.Sources.Select(s => s.Text).Where(t => !string.IsNullOrEmpty(t)));
                if (!string.IsNullOrEmpty(sourcesText)) this.externalCache[emote.Id] = sourcesText;
            }

            this.isReady = true;
            this.logger.Info($"Successfully cached localized emote sources from FFXIV Collect API ({language}).");
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Failed to retrieve emote sources from FFXIV Collect API. Falling back to Lumina data entirely.");
        }
    }

    public string GetUnlockSource(uint emoteId) {
        if (this.isReady && this.externalCache.TryGetValue(emoteId, out var externalSource)) return externalSource;

        return this.fallbackProvider.GetUnlockSource(emoteId);
    }

    public void Dispose() {
        this.httpClient.Dispose();
    }
}