namespace RolePlayer.Tests;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.Framework;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System.IO;
using Xunit;

public class RolePlayerPluginTests {
    [Fact]
    public void Plugin_OnInitialization_BuildsDependencyInjectionWithoutErrors() {
        var services = new ServiceCollection();

        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var tempConfigPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(tempConfigPath));

        services.AddSingleton(mockPluginInterface);
        services.AddSingleton(Substitute.For<IChatGui>());
        services.AddSingleton(Substitute.For<IFramework>());
        services.AddSingleton(Substitute.For<IObjectTable>());
        services.AddSingleton(Substitute.For<ISigScanner>());
        services.AddSingleton(Substitute.For<ITargetManager>());
        services.AddSingleton(Substitute.For<IClientState>());
        services.AddSingleton(Substitute.For<ICommandManager>());
        services.AddSingleton(Substitute.For<IPluginLog>());
        services.AddSingleton(Substitute.For<ITextureProvider>());
        services.AddSingleton(Substitute.For<IGameGui>());

        services.AddSingleton(Substitute.For<ICondition>());
        services.AddSingleton(Substitute.For<IDataManager>());
        services.AddSingleton(Substitute.For<IKeyState>());
        services.AddSingleton(Substitute.For<IGameInteropProvider>());

        // Mock internal core services
        services.AddSingleton(Substitute.For<ILoggerService>());
        services.AddSingleton(Substitute.For<ILocalizationService>());
        services.AddSingleton(Substitute.For<INativeExecutionService>());

        services.AddPluginFeatures();

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        Assert.NotNull(serviceProvider);

        if (Directory.Exists(tempConfigPath)) Directory.Delete(tempConfigPath, true);
    }
}