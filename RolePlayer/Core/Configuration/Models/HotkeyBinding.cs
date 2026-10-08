namespace RolePlayer.Core.Configuration.Models;

using System;

[Serializable]
public class HotkeyBinding {
    public KeyCombination Key { get; set; } = new();
    public ActionReference Action { get; set; } = new();
}