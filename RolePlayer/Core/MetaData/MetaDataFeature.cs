namespace RolePlayer.Core.MetaData;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.Core.MetaData.Services;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class MetaDataFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ITagManagementService, TagManagementService>();
        services.AddSingleton<IGroupManagementService, GroupManagementService>();
    }
}