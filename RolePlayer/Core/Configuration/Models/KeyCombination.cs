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

    public void GetAbbreviatedFormat(out string modifiers, out string mainKey) {
        modifiers = string.Empty;
        if (this.Ctrl) modifiers += "c";
        if (this.Shift) modifiers += "s";
        if (this.Alt) modifiers += "a";

        mainKey = this.Key.ToString();

        // Nettoyage des noms de touches peu esthétiques de l'énumération
        if (mainKey.StartsWith("D") && mainKey.Length == 2 && char.IsDigit(mainKey[1])) {
            mainKey = mainKey.Substring(1);
        }
        else if (mainKey.StartsWith("NUMPAD")) {
            mainKey = "N" + mainKey.Substring(6);
        }
    }

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