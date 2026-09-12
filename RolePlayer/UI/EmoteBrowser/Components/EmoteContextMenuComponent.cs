namespace RolePlayer.UI.EmoteBrowser.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Localization.Contracts;
using System.Linq;

public class EmoteContextMenuComponent {
    private IEmoteExecutionService executionService;
    private ILocalizationService localization;
    private IMacroManagementService macroService;
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;
    private IConfigurationService configurationService;
    private HotbarManagerComponent hotbarManager;

    public EmoteContextMenuComponent(
        IEmoteExecutionService executionService,
        ILocalizationService localization,
        IMacroManagementService macroService,
        IGroupManagementService groupManagementService,
        ITagManagementService tagManagementService,
        IConfigurationService configurationService,
        HotbarManagerComponent hotbarManager) {

        this.executionService = executionService;
        this.localization = localization;
        this.macroService = macroService;
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
        this.configurationService = configurationService;
        this.hotbarManager = hotbarManager;
    }

    public bool Draw(EnrichedEmote emote, EmoteContext context) {
        bool needsFilterApply = false;

        if (ImGui.BeginPopupContextItem($"EmoteContextMenu_{emote.Id}")) {
            bool hasCommand = !string.IsNullOrWhiteSpace(emote.LocalizedCommand);

            if (hasCommand && ImGui.MenuItem(this.localization.Translate("browser_ctx_copy"))) ImGui.SetClipboardText(emote.LocalizedCommand);

            if (ImGui.MenuItem(this.localization.Translate("browser_ctx_execute"), "", false, emote.IsUnlocked)) this.executionService.ExecuteEmote(emote.Id);

            ImGui.Separator();

            if (ImGui.BeginMenu(this.localization.Translate("browser_ctx_append_macro"))) {
                if (!hasCommand) {
                    ImGui.MenuItem(this.localization.Translate("browser_details_no_command"), "", false, false);
                }
                else {
                    var unlockedMacros = this.macroService.GetMacros().Where(m => !m.IsLocked).ToList();
                    if (!unlockedMacros.Any()) {
                        ImGui.MenuItem(this.localization.Translate("browser_ctx_no_unlocked_macros"), "", false, false);
                    }
                    else {
                        foreach (var m in unlockedMacros) {
                            if (ImGui.MenuItem(m.Name)) this.macroService.AppendToMacro(m.Id, emote.LocalizedCommand);
                        }
                    }
                }
                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu(this.localization.Translate("browser_ctx_assign_hotbar"))) {
                var manualHotbars = context.Hotbars.Where(h => h.PopulationMode == HotbarPopulationMode.Manual).ToList();
                if (!manualHotbars.Any()) ImGui.MenuItem(this.localization.Translate("browser_ctx_no_hotbars"), "", false, false);

                bool hotbarChanged = false;
                foreach (var hotbar in manualHotbars) {
                    bool isInHotbar = hotbar.ManualEmoteIds.Contains(emote.Id);
                    if (ImGui.MenuItem(hotbar.Name, "", isInHotbar)) {
                        if (isInHotbar) hotbar.ManualEmoteIds.Remove(emote.Id);
                        else hotbar.ManualEmoteIds.Add(emote.Id);
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
                bool groupChanged = false;
                var currentGroup = this.groupManagementService.GetGroupForEmote(emote.Id);

                if (ImGui.MenuItem(this.localization.Translate("browser_ctx_none"), "", string.IsNullOrEmpty(currentGroup))) {
                    this.groupManagementService.RemoveEmoteFromGroup(emote.Id);
                    groupChanged = true;
                }

                foreach (var group in context.EmoteGroups) {
                    bool isInGroup = currentGroup == group.Name;
                    if (ImGui.MenuItem(group.Name, "", isInGroup)) {
                        this.groupManagementService.AssignEmoteToGroup(emote.Id, group.Name);
                        groupChanged = true;
                    }
                }

                if (groupChanged) needsFilterApply = true;
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(this.localization.Translate("browser_ctx_assign_tag"))) {
                bool tagChanged = false;
                var currentTags = this.tagManagementService.GetTagsForEmote(emote.Id).ToList();

                if (!context.AvailableTags.Any()) ImGui.MenuItem(this.localization.Translate("browser_ctx_no_tags"), "", false, false);

                foreach (var tag in context.AvailableTags) {
                    bool hasTag = currentTags.Contains(tag);
                    if (ImGui.MenuItem(tag, "", hasTag)) {
                        if (hasTag) this.tagManagementService.RemoveTagFromEmote(emote.Id, tag);
                        else this.tagManagementService.AddTagToEmote(emote.Id, tag);
                        tagChanged = true;
                    }
                }

                if (tagChanged) needsFilterApply = true;
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }

        return needsFilterApply;
    }
}