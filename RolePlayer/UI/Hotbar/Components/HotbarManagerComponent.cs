namespace RolePlayer.UI.Hotbar.Components;

using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

public class HotbarManagerComponent : IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IConfigurationService configService;
    private IHotbarResolverService resolverService;
    private IEmoteExecutionService executionService;
    private ITextureProvider textureProvider;
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IModStateProvider modStateProvider;

    private WindowSystem windowSystem;
    private List<EmoteDisplayData> sharedCache = new();

    public HotbarManagerComponent(
        IDalamudPluginInterface pluginInterface,
        IConfigurationService configService,
        IHotbarResolverService resolverService,
        IEmoteExecutionService executionService,
        ITextureProvider textureProvider,
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IModStateProvider modStateProvider) {

        this.pluginInterface = pluginInterface;
        this.configService = configService;
        this.resolverService = resolverService;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.modStateProvider = modStateProvider;

        this.windowSystem = new WindowSystem("RolePlayer_Hotbars");
        this.pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.modStateProvider.ModStateChanged += this.RebuildCache;

        this.RebuildCache();
        this.RefreshWindows();
    }

    private void RebuildCache() {
        var baseEmotes = this.emoteRepository.GetBaseEmotes().ToList();
        var newCache = new List<EmoteDisplayData>();

        foreach (var emote in baseEmotes) {
            emote.IsUnlocked = !emote.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(emote.Id);

            var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
            emote.IsModded = !string.IsNullOrEmpty(modName);
            emote.ModName = modName;

            newCache.Add(emote);
        }

        this.sharedCache = newCache;
    }

    public void RefreshWindows() {
        this.windowSystem.RemoveAllWindows();
        var config = this.configService.GetConfig();

        foreach (var hotbarConfig in config.Hotbars) {
            if (!hotbarConfig.IsVisible) {
                continue;
            }

            var window = new HotbarWindow(
                hotbarConfig,
                this.resolverService,
                this.executionService,
                this.textureProvider,
                () => this.sharedCache
            );
            this.windowSystem.AddWindow(window);
        }
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.modStateProvider.ModStateChanged -= this.RebuildCache;
        this.windowSystem.RemoveAllWindows();
    }
}