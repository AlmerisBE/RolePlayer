namespace RolePlayer.UI.EmoteBrowser;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Services;
using RolePlayer.UI.EmoteBrowser.Tabs;

public class EmoteBrowserFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        // Services d'état
        services.AddSingleton<IEmoteSelectionState, EmoteSelectionState>();

        // Composants UI
        services.AddSingleton<EmoteDetailsPanel>();

        // Onglets (Enregistrés en tant que IEmoteBrowserTab pour l'injection IEnumerable<IEmoteBrowserTab>)
        services.AddSingleton<IEmoteBrowserTab, AllEmotesTab>();
    }
}