namespace RolePlayer.Tests.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.API.Penumbra.Providers;
using System;
using System.Collections.Generic;
using Xunit;

public class PenumbraIpcProviderTests {
    [Fact]
    public void GetModNameModifyingEmote_WhenPenumbraIsNotAvailable_ReturnsEmptyString() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockSubscriber = Substitute.For<ICallGateSubscriber<string, string>>();
        var mockEmotePathProvider = Substitute.For<IEmotePathProvider>();

        mockPluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath").Returns(mockSubscriber);
        mockSubscriber.InvokeFunc(Arg.Any<string>()).Throws(new Exception("IPC not registered"));

        mockEmotePathProvider.GetEmoteGamePaths(Arg.Any<uint>())
            .Returns(new List<string> { "chara/action/emote/e0001.pap" });

        var provider = new PenumbraIpcProvider(mockPluginInterface, mockEmotePathProvider);

        var result = provider.GetModNameModifyingEmote(1);

        Assert.Equal(string.Empty, result);
    }
}