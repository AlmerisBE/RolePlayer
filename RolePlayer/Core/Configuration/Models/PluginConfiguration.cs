namespace RolePlayer.Core.Configuration.Models;

using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using System;
using System.Collections.Generic;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 1;
    public Dictionary<string, CharacterProfile> Profiles { get; set; } = new();

    public VirtualKey Hotkey { get; set; } = VirtualKey.E;
    public bool HotkeyCtrl { get; set; } = true;
    public bool HotkeyShift { get; set; } = false;
    public bool HotkeyAlt { get; set; } = false;
    public string SelectedTheme { get; set; } = "Default";

    public bool EnableHotbars { get; set; } = true;
}