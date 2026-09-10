namespace RolePlayer.Tests.Core.MetaData.Services;

using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.MetaData.Services;
using System;
using System.Collections.Generic;
using Xunit;

public class TagManagementServiceTests {
    [Fact]
    public void RenameGlobalTag_UpdatesTagAcrossAllCollections() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockConfigService = Substitute.For<IConfigurationService>();

        var context = new EmoteContext();
        context.AvailableTags.Add("OldTag");
        context.EmoteTags[1] = new HashSet<string> { "OldTag", "OtherTag" };

        var macroId = Guid.NewGuid();
        context.MacroTags[macroId] = new HashSet<string> { "OldTag" };

        mockContextService.GetCurrentContext().Returns(context);

        var service = new TagManagementService(mockContextService, mockConfigService);

        service.RenameGlobalTag("OldTag", "NewTag");

        Assert.DoesNotContain("OldTag", context.AvailableTags);
        Assert.Contains("NewTag", context.AvailableTags);
        Assert.Contains("NewTag", context.EmoteTags[1]);
        Assert.Contains("NewTag", context.MacroTags[macroId]);
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void AddTagToEmote_CreatesHashSetIfMissingAndAddsTag() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var context = new EmoteContext();

        mockContextService.GetCurrentContext().Returns(context);

        var service = new TagManagementService(mockContextService, mockConfigService);

        service.AddTagToEmote(99, "Smile");

        Assert.True(context.EmoteTags.ContainsKey(99));
        Assert.Contains("Smile", context.EmoteTags[99]);
        mockConfigService.Received(1).Save();
    }

    [Fact]
    public void DeleteGlobalTag_RemovesTagFromAllCollections() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockConfigService = Substitute.For<IConfigurationService>();

        var context = new EmoteContext();
        context.AvailableTags.Add("ToTrash");
        context.EmoteTags[1] = new HashSet<string> { "ToTrash", "KeepMe" };

        mockContextService.GetCurrentContext().Returns(context);

        var service = new TagManagementService(mockContextService, mockConfigService);

        service.DeleteGlobalTag("ToTrash");

        Assert.DoesNotContain("ToTrash", context.AvailableTags);
        Assert.DoesNotContain("ToTrash", context.EmoteTags[1]);
        Assert.Contains("KeepMe", context.EmoteTags[1]);
        mockConfigService.Received(1).Save();
    }
}