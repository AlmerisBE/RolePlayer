namespace RolePlayer.Tests;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.Core.Configuration.Models;
using System;
using System.IO;
using Xunit;

public class RolePlayerPluginTests {

    [Fact]
    public void Plugin_OnInitialization_BuildsDependencyInjectionWithoutErrors() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockChatGui = Substitute.For<IChatGui>();
        var mockCommandManager = Substitute.For<ICommandManager>();
        var mockClientState = Substitute.For<IClientState>();
        var mockLogger = Substitute.For<IPluginLog>();
        var mockDataManager = Substitute.For<IDataManager>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockInteropProvider = Substitute.For<IGameInteropProvider>();
        var mockTextureProvider = Substitute.For<ITextureProvider>();
        var mockFramework = Substitute.For<IFramework>();
        var mockCondition = Substitute.For<ICondition>();
        var mockKeyState = Substitute.For<IKeyState>();

        // 1. Simule le répertoire pour le ThemeManagementService
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(Path.GetTempPath()));

        // 2. Simule la version du manifeste pour la MainWindow
        mockPluginInterface.Manifest.AssemblyVersion.Returns(new Version("1.0.0.0"));

        // 3. Simule la configuration de base pour le ConfigurationService
        mockPluginInterface.GetPluginConfig().Returns(new PluginConfiguration());

        var exception = Record.Exception(() => new RolePlayerPlugin(
            mockPluginInterface,
            mockChatGui,
            mockCommandManager,
            mockClientState,
            mockLogger,
            mockDataManager,
            mockObjectTable,
            mockInteropProvider,
            mockTextureProvider,
            mockFramework,
            mockCondition,
            mockKeyState));

        Assert.Null(exception);
    }
}