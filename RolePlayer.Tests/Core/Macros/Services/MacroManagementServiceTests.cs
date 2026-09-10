namespace RolePlayer.Tests.Core.Macros.Services;

using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Macros.Models;
using RolePlayer.Core.Macros.Services;
using Xunit;

public class MacroManagementServiceTests {
    [Fact]
    public void AppendToMacro_AddsCommandWithNewline_WhenContentIsNotEmpty() {
        var mockConfigService = Substitute.For<IConfigurationService>();
        var profile = new CharacterProfile();
        var macro = new RoleplayMacro { Content = "/bow" };

        profile.Macros.Add(macro);
        mockConfigService.GetCurrentProfile().Returns(profile);

        var service = new MacroManagementService(mockConfigService);
        service.AppendToMacro(macro.Id, "/sit");

        Assert.Equal("/bow\n/sit", macro.Content);
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void AppendToMacro_DoesNotAddNewline_WhenContentIsEmpty() {
        var mockConfigService = Substitute.For<IConfigurationService>();
        var profile = new CharacterProfile();
        var macro = new RoleplayMacro { Content = string.Empty };

        profile.Macros.Add(macro);
        mockConfigService.GetCurrentProfile().Returns(profile);

        var service = new MacroManagementService(mockConfigService);
        service.AppendToMacro(macro.Id, "/sit");

        Assert.Equal("/sit", macro.Content);
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void AppendToMacro_DoesNothing_WhenMacroIsLocked() {
        var mockConfigService = Substitute.For<IConfigurationService>();
        var profile = new CharacterProfile();
        var macro = new RoleplayMacro { Content = "/bow", IsLocked = true };

        profile.Macros.Add(macro);
        mockConfigService.GetCurrentProfile().Returns(profile);

        var service = new MacroManagementService(mockConfigService);
        service.AppendToMacro(macro.Id, "/sit");

        Assert.Equal("/bow", macro.Content);
        mockConfigService.DidNotReceive().Save();
    }
}