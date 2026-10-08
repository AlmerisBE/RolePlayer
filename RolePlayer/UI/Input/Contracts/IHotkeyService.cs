namespace RolePlayer.UI.Input.Contracts;

using RolePlayer.Core.Configuration.Models;
using System;

public interface IHotkeyService {
    event Action? OnHotkeyPressed;

    void RegisterHotkey(KeyCombination key, ActionReference action);
    void UnregisterHotkey(KeyCombination key);
    KeyCombination? GetAssignedKey(ActionReference action);
    bool IsKeyAssigned(KeyCombination key);
}