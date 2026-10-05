namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.Core.Configuration.Models;
using RolePlayer.UI.Hotbar.Windows;

public interface IHotbarWindowFactory {
    HotbarWindow Create(HotbarConfig config);
}