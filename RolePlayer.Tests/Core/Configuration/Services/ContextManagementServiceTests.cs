namespace RolePlayer.Tests.Core.Configuration.Services;

using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Configuration.Services;
using Xunit;

public class ContextManagementServiceTests {
    [Fact]
    public void GetCurrentContext_WhenActiveContextExists_ReturnsActiveContext() {
        var mockConfigService = Substitute.For<IConfigurationService>();
        var config = new PluginConfiguration();
        var profile = new CharacterProfile();
        var context = new EmoteContext { Name = "Test Context" };

        config.Contexts.Add(context.Id, context);
        profile.ActiveContextId = context.Id;

        mockConfigService.GetConfig().Returns(config);
        mockConfigService.GetCurrentProfile().Returns(profile);

        var service = new ContextManagementService(mockConfigService);
        var result = service.GetCurrentContext();

        Assert.Equal(context.Id, result.Id);
        Assert.Equal("Test Context", result.Name);
    }

    [Fact]
    public void CreateContext_AddsNewContextAndSaves() {
        var mockConfigService = Substitute.For<IConfigurationService>();
        var config = new PluginConfiguration();
        var profile = new CharacterProfile();

        var defaultContext = new EmoteContext { Name = "Default" };
        config.Contexts.Add(defaultContext.Id, defaultContext);
        profile.ActiveContextId = defaultContext.Id;

        mockConfigService.GetConfig().Returns(config);
        mockConfigService.GetCurrentProfile().Returns(profile);

        var service = new ContextManagementService(mockConfigService);
        service.CreateContext("New RP Context", null);

        Assert.Equal(2, config.Contexts.Count);
        Assert.Contains(config.Contexts.Values, c => c.Name == "New RP Context");
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void SwitchContext_ChangesActiveIdAndTriggersEvent() {
        var mockConfigService = Substitute.For<IConfigurationService>();
        var config = new PluginConfiguration();
        var profile = new CharacterProfile();

        var context1 = new EmoteContext { Name = "First Context" };
        var context2 = new EmoteContext { Name = "Second Context" };
        config.Contexts.Add(context1.Id, context1);
        config.Contexts.Add(context2.Id, context2);
        profile.ActiveContextId = context1.Id;

        mockConfigService.GetConfig().Returns(config);
        mockConfigService.GetCurrentProfile().Returns(profile);

        var service = new ContextManagementService(mockConfigService);
        bool eventTriggered = false;
        service.ContextChanged += () => eventTriggered = true;

        service.SwitchContext(context2.Id);

        Assert.Equal(context2.Id, profile.ActiveContextId);
        Assert.True(eventTriggered);
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void DeleteContext_RemovesContextAndSwitchesIfActive() {
        var mockConfigService = Substitute.For<IConfigurationService>();
        var config = new PluginConfiguration();
        var profile = new CharacterProfile();

        var context1 = new EmoteContext { Name = "First Context" };
        var context2 = new EmoteContext { Name = "To Delete" };
        config.Contexts.Add(context1.Id, context1);
        config.Contexts.Add(context2.Id, context2);
        profile.ActiveContextId = context2.Id;

        mockConfigService.GetConfig().Returns(config);
        mockConfigService.GetCurrentProfile().Returns(profile);

        var service = new ContextManagementService(mockConfigService);

        service.DeleteContext(context2.Id);

        Assert.Single(config.Contexts);
        Assert.Equal(context1.Id, profile.ActiveContextId);
        mockConfigService.Received(1).Save();
    }
}