namespace RolePlayer.UI.MainWindow.Components.Macros;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Localization.Contracts;
using System.Linq;

public class MacroContextMenuComponent {
    private IMacroExecutionService macroExecutionService;
    private ILocalizationService localization;
    private IConfigurationService configurationService;
    private IContextManagementService contextService;
    private IGroupManagementService groupService;
    private ITagManagementService tagService;
    private HotbarManagerComponent hotbarManager;

    public MacroContextMenuComponent(
        IMacroExecutionService macroExecutionService,
        ILocalizationService localization,
        IConfigurationService configurationService,
        IContextManagementService contextService,
        IGroupManagementService groupService,
        ITagManagementService tagService,
        HotbarManagerComponent hotbarManager) {

        this.macroExecutionService = macroExecutionService;
        this.localization = localization;
        this.configurationService = configurationService;
        this.contextService = contextService;
        this.groupService = groupService;
        this.tagService = tagService;
        this.hotbarManager = hotbarManager;
    }

    public void Draw(RoleplayMacro macro) {
        if (ImGui.BeginPopupContextItem($"MacroContextMenu_{macro.Id}")) {
            if (ImGui.MenuItem(this.localization.Translate("config_macro_copy"))) ImGui.SetClipboardText(macro.Content);

            if (ImGui.MenuItem(this.localization.Translate("browser_ctx_execute"))) this.macroExecutionService.Execute(macro);

            ImGui.Separator();

            var context = this.contextService.GetCurrentContext();

            if (ImGui.BeginMenu(this.localization.Translate("browser_ctx_assign_hotbar"))) {
                var manualHotbars = context.Hotbars.Where(h => h.PopulationMode == HotbarPopulationMode.Manual).ToList();
                if (!manualHotbars.Any()) ImGui.MenuItem(this.localization.Translate("browser_ctx_no_hotbars"), "", false, false);

                bool hotbarChanged = false;
                foreach (var hotbar in manualHotbars) {
                    bool isInHotbar = hotbar.ManualMacroIds.Contains(macro.Id);
                    if (ImGui.MenuItem(hotbar.Name, "", isInHotbar)) {
                        if (isInHotbar) hotbar.ManualMacroIds.Remove(macro.Id);
                        else hotbar.ManualMacroIds.Add(macro.Id);
                        hotbarChanged = true;
                    }
                }

                if (hotbarChanged) {
                    this.configurationService.Save();
                    this.hotbarManager.RefreshWindows();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(this.localization.Translate("browser_ctx_assign_group"))) {
                var currentGroup = this.groupService.GetGroupForMacro(macro.Id);

                if (ImGui.MenuItem(this.localization.Translate("browser_ctx_none"), "", string.IsNullOrEmpty(currentGroup))) {
                    this.groupService.RemoveMacroFromGroup(macro.Id);
                }

                foreach (var group in context.EmoteGroups) {
                    bool isInGroup = currentGroup == group.Name;
                    if (ImGui.MenuItem(group.Name, "", isInGroup)) {
                        this.groupService.AssignMacroToGroup(macro.Id, group.Name);
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(this.localization.Translate("browser_ctx_assign_tag"))) {
                var currentTags = this.tagService.GetTagsForMacro(macro.Id).ToList();

                if (!context.AvailableTags.Any()) ImGui.MenuItem(this.localization.Translate("browser_ctx_no_tags"), "", false, false);

                foreach (var tag in context.AvailableTags) {
                    bool hasTag = currentTags.Contains(tag);
                    if (ImGui.MenuItem(tag, "", hasTag)) {
                        if (hasTag) this.tagService.RemoveTagFromMacro(macro.Id, tag);
                        else this.tagService.AddTagToMacro(macro.Id, tag);
                    }
                }
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }
}