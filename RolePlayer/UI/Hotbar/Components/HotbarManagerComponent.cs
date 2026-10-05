namespace RolePlayer.UI.Hotbar.Components;

using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

public class HotbarManagerComponent : IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IContextManagementService contextService;
    private IEmoteCache emoteCache;
    private IHotbarWindowFactory windowFactory;

    private WindowSystem windowSystem;
    private Dictionary<Guid, HotbarWindow> activeWindows = new();
    private bool needsRefresh = false;

    public HotbarManagerComponent(
        IDalamudPluginInterface pluginInterface,
        IContextManagementService contextService,
        IEmoteCache emoteCache,
        IHotbarWindowFactory windowFactory) {

        this.pluginInterface = pluginInterface;
        this.contextService = contextService;
        this.emoteCache = emoteCache;
        this.windowFactory = windowFactory;

        this.windowSystem = new WindowSystem("RolePlayer_Hotbars");

        this.pluginInterface.UiBuilder.Draw += this.OnDraw;

        this.emoteCache.CacheUpdated += this.RequestRefresh;
        this.contextService.ContextChanged += this.RequestRefresh;
        this.contextService.HotbarsChanged += this.RequestRefresh;

        this.RequestRefresh();
    }

    public void RequestRefresh() {
        this.needsRefresh = true;
    }

    private void OnDraw() {
        if (this.needsRefresh) {
            this.PerformRefreshWindows();
            this.needsRefresh = false;
        }

        this.windowSystem.Draw();
    }

    private void PerformRefreshWindows() {
        var context = this.contextService.GetCurrentContext();
        var validIds = new HashSet<Guid>();

        foreach (var hotbarConfig in context.Hotbars) {
            validIds.Add(hotbarConfig.Id);

            if (this.activeWindows.TryGetValue(hotbarConfig.Id, out var existingWindow)) {
                existingWindow.UpdateConfig(hotbarConfig);
            }
            else {
                var window = this.windowFactory.Create(hotbarConfig);
                this.windowSystem.AddWindow(window);
                this.activeWindows[hotbarConfig.Id] = window;
            }
        }

        var toRemove = this.activeWindows.Keys.Except(validIds).ToList();
        foreach (var id in toRemove) {
            var windowToRemove = this.activeWindows[id];
            this.windowSystem.RemoveWindow(windowToRemove);
            this.activeWindows.Remove(id);
        }
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.OnDraw;
        this.emoteCache.CacheUpdated -= this.RequestRefresh;
        this.contextService.ContextChanged -= this.RequestRefresh;
        this.contextService.HotbarsChanged -= this.RequestRefresh;

        foreach (var window in this.activeWindows.Values) {
            this.windowSystem.RemoveWindow(window);
        }

        this.activeWindows.Clear();
    }
}