namespace RolePlayer.Core.Configuration.Models;

using Dalamud.Game.ClientState.Keys;
using System;
using System.Collections.Generic;

[Serializable]
public class KeyCombination : IEquatable<KeyCombination> {
    public VirtualKey Key { get; set; }
    public bool Ctrl { get; set; }
    public bool Shift { get; set; }
    public bool Alt { get; set; }

    public bool Equals(KeyCombination? other) {
        if (other is null) return false;
        return this.Key == other.Key && this.Ctrl == other.Ctrl && this.Shift == other.Shift && this.Alt == other.Alt;
    }

    public override bool Equals(object? obj) => this.Equals(obj as KeyCombination);
    public override int GetHashCode() => HashCode.Combine(this.Key, this.Ctrl, this.Shift, this.Alt);

    public override string ToString() {
        var parts = new List<string>();
        if (this.Ctrl) parts.Add("Ctrl");
        if (this.Shift) parts.Add("Shift");
        if (this.Alt) parts.Add("Alt");
        parts.Add(this.Key.ToString());
        return string.Join(" + ", parts);
    }
}