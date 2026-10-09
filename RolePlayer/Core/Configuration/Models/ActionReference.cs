namespace RolePlayer.Core.Configuration.Models;

using System;

[Serializable]
public class ActionReference : IEquatable<ActionReference> {
    public ActionType Type { get; set; }
    public uint EmoteId { get; set; }
    public Guid MacroId { get; set; }

    public bool Equals(ActionReference? other) {
        if (other is null) return false;
        return this.Type == other.Type && this.EmoteId == other.EmoteId && this.MacroId == other.MacroId;
    }

    public override bool Equals(object? obj) => this.Equals(obj as ActionReference);
    public override int GetHashCode() => HashCode.Combine(this.Type, this.EmoteId, this.MacroId);
}