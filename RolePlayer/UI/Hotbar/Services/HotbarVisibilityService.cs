namespace RolePlayer.UI.Hotbar.Services;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.UI.Hotbar.Contracts;

public class HotbarVisibilityService : IHotbarVisibilityService {
    private IConfigurationService configService;
    private ICondition condition;

    public HotbarVisibilityService(IConfigurationService configService, ICondition condition) {
        this.configService = configService;
        this.condition = condition;
    }

    public bool ShouldHide(HotbarConfig config) {
        if (!this.configService.GetConfig().EnableHotbars) return true;
        if (this.condition[ConditionFlag.WatchingCutscene]) return true;
        if (this.condition[ConditionFlag.BetweenAreas] || this.condition[ConditionFlag.BetweenAreas51]) return true;
        if (this.condition[ConditionFlag.LoggingOut]) return true;
        if (config.HideInCombat && this.condition[ConditionFlag.InCombat]) return true;
        if (config.HideInDuty && (this.condition[ConditionFlag.BoundByDuty] || this.condition[ConditionFlag.BoundByDuty56])) return true;

        return false;
    }
}