namespace RolePlayer.Tests.Core.Configuration.Services;

using Dalamud.Configuration;
using Dalamud.Plugin;
using NSubstitute;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Configuration.Services;
using Xunit;

public class ConfigurationServiceTests {
    [Fact]
    public void ConfigurationService_Initialization_LoadsExistingConfig() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var existingConfig = new PluginConfiguration {
            Version = 1
        };

        mockPluginInterface.GetPluginConfig().Returns(existingConfig);

        var service = new ConfigurationService(mockPluginInterface);
        var config = service.GetConfig();

        Assert.NotNull(config);
        Assert.Equal(1, config.Version);
    }

    [Fact]
    public void ConfigurationService_Initialization_CreatesNewConfigIfNull() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        mockPluginInterface.GetPluginConfig().Returns((IPluginConfiguration)null!);

        var service = new ConfigurationService(mockPluginInterface);
        var config = service.GetConfig();

        Assert.NotNull(config);
        Assert.Equal(0, config.Version);
    }

    [Fact]
    public void ConfigurationService_Save_PassesConfigToDalamud() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var service = new ConfigurationService(mockPluginInterface);
        var config = service.GetConfig();

        config.Version = 2;
        service.Save();

        mockPluginInterface.Received(1).SavePluginConfig(config);
    }
}