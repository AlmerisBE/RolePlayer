namespace RolePlayer.Tests.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RolePlayer.API.Penumbra.Providers;
using System;
using Xunit;

public class PenumbraIpcProviderTests {
    [Fact]
    public void GetModNameModifyingEmote_WhenPenumbraIsNotAvailable_ReturnsEmptyString() {
        // Arrange
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockSubscriber = Substitute.For<ICallGateSubscriber<string, string>>();

        mockPluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath").Returns(mockSubscriber);

        // Simuler l'absence de Penumbra (l'IPC lance une exception)
        mockSubscriber.InvokeFunc(Arg.Any<string>()).Throws(new Exception("IPC not registered"));

        var provider = new PenumbraIpcProvider(mockPluginInterface);

        // Act
        var result = provider.GetModNameModifyingEmote(1);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}