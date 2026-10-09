namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.UI.Hotbar.Models;

public interface IHotbarButtonRenderer {
    void Draw(ResolvedHotbarItem item, float iconSize);
}