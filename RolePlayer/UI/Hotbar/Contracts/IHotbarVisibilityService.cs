namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.Core.Configuration.Models;

public interface IHotbarVisibilityService {
    bool ShouldHide(HotbarConfig config);
}