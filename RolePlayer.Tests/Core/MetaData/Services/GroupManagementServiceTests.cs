namespace RolePlayer.Tests.Core.MetaData.Services;

using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.Core.MetaData.Services;
using System.Linq;
using Xunit;

public class GroupManagementServiceTests {
    [Fact]
    public void CreateGroup_AddsGroupToContext_WhenNameIsValidAndUnique() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var context = new EmoteContext();

        mockContextService.GetCurrentContext().Returns(context);

        var service = new GroupManagementService(mockContextService, mockConfigService);
        var newGroup = new EmoteGroup { Name = "Favorites", Description = "My favorite emotes" };

        service.CreateGroup(newGroup);

        Assert.Single(context.EmoteGroups);
        Assert.Equal("Favorites", context.EmoteGroups.First().Name);
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void CreateGroup_DoesNotAddGroup_WhenNameAlreadyExists() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var context = new EmoteContext();
        context.EmoteGroups.Add(new EmoteGroup { Name = "Favorites" });

        mockContextService.GetCurrentContext().Returns(context);

        var service = new GroupManagementService(mockContextService, mockConfigService);
        var newGroup = new EmoteGroup { Name = "favorites" };

        service.CreateGroup(newGroup);

        Assert.Single(context.EmoteGroups);
        mockConfigService.DidNotReceive().Save();
    }

    [Fact]
    public void AssignEmoteToGroup_UpdatesEmoteToGroupMapAndSaves() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var context = new EmoteContext();

        mockContextService.GetCurrentContext().Returns(context);

        var service = new GroupManagementService(mockContextService, mockConfigService);

        service.AssignEmoteToGroup(42, "Favorites");

        Assert.True(context.EmoteToGroupMap.ContainsKey(42));
        Assert.Equal("Favorites", context.EmoteToGroupMap[42]);
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void DeleteGroup_RemovesGroupFromContextAndSaves() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var context = new EmoteContext();
        context.EmoteGroups.Add(new EmoteGroup { Name = "Favorites" });

        mockContextService.GetCurrentContext().Returns(context);

        var service = new GroupManagementService(mockContextService, mockConfigService);

        service.DeleteGroup("Favorites");

        Assert.Empty(context.EmoteGroups);
        mockConfigService.Received(1).Save();
    }
}