namespace RolePlayer.Tests.UI.Input.Services;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Input.Services;
using System;
using Xunit;

public class HotkeyServiceTests {
    private IKeyState keyState;
    private IDalamudPluginInterface pluginInterface;
    private IConfigurationService configService;
    private IEmoteExecutionService emoteExecution;
    private IMacroExecutionService macroExecution;
    private IMacroManagementService macroManagement;
    private CharacterProfile profile;

    public HotkeyServiceTests() {
        this.keyState = Substitute.For<IKeyState>();

        this.pluginInterface = Substitute.For<IDalamudPluginInterface>();
        this.pluginInterface.UiBuilder.Returns(Substitute.For<IUiBuilder>());

        this.configService = Substitute.For<IConfigurationService>();
        this.emoteExecution = Substitute.For<IEmoteExecutionService>();
        this.macroExecution = Substitute.For<IMacroExecutionService>();
        this.macroManagement = Substitute.For<IMacroManagementService>();

        this.profile = new CharacterProfile();
        this.configService.GetCurrentProfile().Returns(this.profile);
    }

    [Fact]
    public void RegisterHotkey_OverwritesExisting_WhenKeyCombinationIsIdentical() {
        var key = new KeyCombination { Key = VirtualKey.F1, Ctrl = true };
        var action1 = new ActionReference { Type = ActionType.Emote, EmoteId = 50 };
        var action2 = new ActionReference { Type = ActionType.Macro, MacroId = Guid.NewGuid() };

        var service = new HotkeyService(this.keyState, this.pluginInterface, this.configService, this.emoteExecution, this.macroExecution, this.macroManagement);

        service.RegisterHotkey(key, action1);
        Assert.Single(this.profile.Hotkeys);
        Assert.Equal(action1, this.profile.Hotkeys[0].Action);

        service.RegisterHotkey(key, action2);
        Assert.Single(this.profile.Hotkeys);
        Assert.Equal(action2, this.profile.Hotkeys[0].Action);

        this.configService.Received(2).Save();
    }

    [Fact]
    public void GetAssignedKey_ReturnsCorrectCombination_WhenActionExists() {
        var key = new KeyCombination { Key = VirtualKey.F5 };
        var action = new ActionReference { Type = ActionType.Emote, EmoteId = 42 };
        this.profile.Hotkeys.Add(new HotkeyBinding { Key = key, Action = action });

        var service = new HotkeyService(this.keyState, this.pluginInterface, this.configService, this.emoteExecution, this.macroExecution, this.macroManagement);

        var result = service.GetAssignedKey(action);

        Assert.NotNull(result);
        Assert.Equal(VirtualKey.F5, result.Key);
    }
}