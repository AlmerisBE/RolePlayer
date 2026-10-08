namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Input.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System.Linq;
using System.Numerics;

public class HotkeysConfigSubTab {
    private IConfigurationService configService;
    private IHotkeyService hotkeyService;
    private ILocalizationService localization;
    private IEmoteCache emoteCache;
    private IMacroManagementService macroService;

    public string Name => this.localization.Translate("config_tab_hotkeys");

    public HotkeysConfigSubTab(
        IConfigurationService configService,
        IHotkeyService hotkeyService,
        ILocalizationService localization,
        IEmoteCache emoteCache,
        IMacroManagementService macroService) {

        this.configService = configService;
        this.hotkeyService = hotkeyService;
        this.localization = localization;
        this.emoteCache = emoteCache;
        this.macroService = macroService;
    }

    public void Draw() {
        var profile = this.configService.GetCurrentProfile();

        ImGui.TextDisabled(this.localization.Translate("config_hotkeys_description"));
        ImGui.Spacing();

        if (ImGui.BeginTable("HotkeysListTable", 4, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_key"), ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_type"), ImGuiTableColumnFlags.WidthFixed, 80f);
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_action"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_hotkeys_col_delete"), ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableHeadersRow();

            var keysToRemove = new System.Collections.Generic.List<RolePlayer.Core.Configuration.Models.KeyCombination>();

            foreach (var binding in profile.Hotkeys) {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(binding.Key.ToString());

                ImGui.TableNextColumn();
                string typeStr = binding.Action.Type.ToString();
                ImGui.TextDisabled(typeStr);

                ImGui.TableNextColumn();
                string actionName = this.localization.Translate("browser_unknown");

                if (binding.Action.Type == RolePlayer.Core.Configuration.Models.ActionType.Emote) {
                    var emote = this.emoteCache.GetCachedEmotes().FirstOrDefault(e => e.Id == binding.Action.EmoteId);
                    if (emote != null) actionName = emote.Name;
                }
                else if (binding.Action.Type == RolePlayer.Core.Configuration.Models.ActionType.Macro) {
                    var macro = this.macroService.GetMacros().FirstOrDefault(m => m.Id == binding.Action.MacroId);
                    if (macro != null) actionName = macro.Name;
                }

                ImGui.TextUnformatted(actionName);

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##{binding.Key.GetHashCode()}", new Vector2(-1, 0))) {
                    keysToRemove.Add(binding.Key);
                }
                ImGui.PopFont();
            }

            ImGui.EndTable();

            foreach (var key in keysToRemove) {
                this.hotkeyService.UnregisterHotkey(key);
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Emplacement réservé pour le bouton d'ajout "+" dans la prochaine itération.
        if (ImGui.Button(this.localization.Translate("config_hotkeys_add_new"), new Vector2(-1, 30))) {
            // Logique de création à venir (Modale d'écoute)
        }
    }
}