namespace RolePlayer.UI.EmoteBrowser;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Providers;
using RolePlayer.UI.EmoteBrowser.Services;
using RolePlayer.UI.EmoteBrowser.Tabs;
using RolePlayer.UI.Localization.Contracts;

public class EmoteBrowserFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILocalizationProvider, EmoteBrowserLocalizationProvider>();

        services.AddSingleton<EmoteFilterComponent>();
        services.AddSingleton<EmoteDetailsPanel>();
        services.AddSingleton<IEmoteSelectionState, EmoteSelectionState>();
        services.AddSingleton<IEmoteBrowserTab, AllEmotesTab>();
    }
}