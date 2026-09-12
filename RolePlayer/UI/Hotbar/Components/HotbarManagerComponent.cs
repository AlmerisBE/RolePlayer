namespace RolePlayer.UI.Hotbar.Components;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Hotbar.Windows;
using RolePlayer.UI.Localization.Contracts;
using System;

public class HotbarManagerComponent : IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IConfigurationService configService;
    private IContextManagementService contextService;
    private IHotbarResolverService resolverService;
    private IEmoteExecutionService executionService;
    private IMacroExecutionService macroExecutionService;
    private ITextureProvider textureProvider;
    private IEmoteCache emoteCache;
    private ICondition condition;
    private ILocalizationService localization;

    private WindowSystem windowSystem;

    public HotbarManagerComponent(
        IDalamudPluginInterface pluginInterface,
        IConfigurationService configService,
        IContextManagementService contextService,
        IHotbarResolverService resolverService,
        IEmoteExecutionService executionService,
        IMacroExecutionService macroExecutionService,
        ITextureProvider textureProvider,
        IEmoteCache emoteCache,
        ICondition condition,
        ILocalizationService localization) {

        this.pluginInterface = pluginInterface;
        this.configService = configService;
        this.contextService = contextService;
        this.resolverService = resolverService;
        this.executionService = executionService;
        this.macroExecutionService = macroExecutionService;
        this.textureProvider = textureProvider;
        this.emoteCache = emoteCache;
        this.condition = condition;
        this.localization = localization;

        this.windowSystem = new WindowSystem("RolePlayer_Hotbars");
        this.pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;

        this.emoteCache.CacheUpdated += this.RefreshWindows;
        this.configService.ProfileLoaded += this.RefreshWindows;
        this.contextService.ContextChanged += this.RefreshWindows;
        this.contextService.HotbarsChanged += this.RefreshWindows;

        this.RefreshWindows();
    }

    private bool EvaluateHotbarVisibility(HotbarConfig config) {
        if (!this.configService.GetConfig().EnableHotbars) return true;
        if (this.condition[ConditionFlag.WatchingCutscene]) return true;
        if (config.HideInCombat && this.condition[ConditionFlag.InCombat]) return true;
        if (config.HideInDuty && (this.condition[ConditionFlag.BoundByDuty] || this.condition[ConditionFlag.BoundByDuty56])) return true;

        return false;
    }

    public void RefreshWindows() {
        this.windowSystem.RemoveAllWindows();
        var context = this.contextService.GetCurrentContext();

        foreach (var hotbarConfig in context.Hotbars) {
            if (!hotbarConfig.IsVisible) continue;

            var window = new HotbarWindow(
                hotbarConfig,
                this.resolverService,
                this.executionService,
                this.macroExecutionService,
                this.textureProvider,
                () => this.emoteCache.GetCachedEmotes(),
                () => this.EvaluateHotbarVisibility(hotbarConfig),
                this.localization
            );
            this.windowSystem.AddWindow(window);
        }
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.emoteCache.CacheUpdated -= this.RefreshWindows;
        this.configService.ProfileLoaded -= this.RefreshWindows;
        this.contextService.ContextChanged -= this.RefreshWindows;
        this.contextService.HotbarsChanged -= this.RefreshWindows;
        this.windowSystem.RemoveAllWindows();
    }
}