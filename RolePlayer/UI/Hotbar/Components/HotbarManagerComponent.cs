namespace RolePlayer.UI.Hotbar.Components;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Hotbar.Windows;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class HotbarManagerComponent : IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IConfigurationService configService;
    private IContextManagementService contextService;
    private IHotbarResolverService resolverService;
    private IEmoteExecutionService executionService;
    private ITextureProvider textureProvider;
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IModStateProvider modStateProvider;
    private ICondition condition;
    private IFramework framework;
    private ILocalizationService localization;

    private WindowSystem windowSystem;
    private List<EmoteDisplayData> sharedCache = new();
    private DateTime? pendingRebuildTime = null;

    public HotbarManagerComponent(
        IDalamudPluginInterface pluginInterface,
        IConfigurationService configService,
        IContextManagementService contextService,
        IHotbarResolverService resolverService,
        IEmoteExecutionService executionService,
        ITextureProvider textureProvider,
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IModStateProvider modStateProvider,
        ICondition condition,
        IFramework framework,
        ILocalizationService localization) {

        this.pluginInterface = pluginInterface;
        this.configService = configService;
        this.contextService = contextService;
        this.resolverService = resolverService;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.modStateProvider = modStateProvider;
        this.condition = condition;
        this.framework = framework;
        this.localization = localization;

        this.windowSystem = new WindowSystem("RolePlayer_Hotbars");
        this.pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.modStateProvider.ModStateChanged += this.RebuildCache;
        this.configService.ProfileLoaded += this.OnProfileLoaded;
        this.contextService.ContextChanged += this.OnContextChanged;

        this.playerStateProvider.PlayerStateValid += this.OnPlayerStateValid;
        this.framework.Update += this.OnFrameworkUpdate;

        this.RebuildCache();
        this.RefreshWindows();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (this.pendingRebuildTime.HasValue && DateTime.Now >= this.pendingRebuildTime.Value) {
            this.pendingRebuildTime = null;
            this.RebuildCache();
            this.RefreshWindows();
        }
    }

    private void OnPlayerStateValid() => this.pendingRebuildTime = DateTime.Now.AddSeconds(2);

    private void OnProfileLoaded() {
        this.RebuildCache();
        this.RefreshWindows();
    }

    private void OnContextChanged() {
        this.RebuildCache();
        this.RefreshWindows();
    }

    private bool EvaluateHotbarVisibility(HotbarConfig config) {
        if (!this.configService.GetConfig().EnableHotbars) return true;
        if (this.condition[ConditionFlag.WatchingCutscene]) return true;
        if (config.HideInCombat && this.condition[ConditionFlag.InCombat]) return true;
        if (config.HideInDuty && (this.condition[ConditionFlag.BoundByDuty] || this.condition[ConditionFlag.BoundByDuty56])) return true;

        return false;
    }

    private void RebuildCache() {
        if (!this.playerStateProvider.IsPlayerValid) return;

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

    public IReadOnlyList<EmoteDisplayData> GetEmoteCache() => this.sharedCache;

    public void RefreshWindows() {
        this.windowSystem.RemoveAllWindows();
        var context = this.contextService.GetCurrentContext();

        foreach (var hotbarConfig in context.Hotbars) {
            if (!hotbarConfig.IsVisible) continue;

            var window = new HotbarWindow(
                hotbarConfig,
                this.resolverService,
                this.executionService,
                this.textureProvider,
                () => this.sharedCache,
                () => this.EvaluateHotbarVisibility(hotbarConfig),
                this.localization
            );
            this.windowSystem.AddWindow(window);
        }
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.modStateProvider.ModStateChanged -= this.RebuildCache;
        this.configService.ProfileLoaded -= this.OnProfileLoaded;
        this.contextService.ContextChanged -= this.OnContextChanged;
        this.playerStateProvider.PlayerStateValid -= this.OnPlayerStateValid;
        this.framework.Update -= this.OnFrameworkUpdate;
        this.windowSystem.RemoveAllWindows();
    }
}