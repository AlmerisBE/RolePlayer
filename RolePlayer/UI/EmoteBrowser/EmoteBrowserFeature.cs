namespace RolePlayer.UI.EmoteBrowser;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Services;
using RolePlayer.UI.EmoteBrowser.Tabs;

public class EmoteBrowserFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        // Enregistrement de notre nouveau composant de filtrage
        services.AddSingleton<EmoteFilterComponent>();

        // (Assure-toi que le reste de tes services existants sont bien présents)
        services.AddSingleton<EmoteDetailsPanel>();
        services.AddSingleton<IEmoteSelectionState, EmoteSelectionState>();
        services.AddSingleton<IEmoteBrowserTab, AllEmotesTab>();
    }
}