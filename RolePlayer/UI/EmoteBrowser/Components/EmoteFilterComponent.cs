namespace RolePlayer.UI.EmoteBrowser.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Localization.Contracts;
using System.Collections.Generic;
using System.Linq;

public class EmoteFilterComponent {
    private IConfigurationService configurationService;
    private IContextManagementService contextService;
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;
    private ILocalizationService localization;

    public EmoteFilterComponent(
        IConfigurationService configurationService,
        IContextManagementService contextService,
        IGroupManagementService groupManagementService,
        ITagManagementService tagManagementService,
        ILocalizationService localization) {

        this.configurationService = configurationService;
        this.contextService = contextService;
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
        this.localization = localization;
    }

    public void Draw(IEmoteBrowserPresenter presenter) {
        var context = this.contextService.GetCurrentContext();
        bool filtersChanged = false;

        ImGui.PushFont(UiBuilder.IconFont);
        var filterIconText = FontAwesomeIcon.Filter.ToIconString();
        var filterIconWidth = ImGui.CalcTextSize(filterIconText).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - filterIconWidth - ImGui.GetStyle().ItemSpacing.X);

        string currentQuery = presenter.SearchQuery;
        if (ImGui.InputTextWithHint("##SearchEmotes", this.localization.Translate("browser_search_hint"), ref currentQuery, 128)) {
            presenter.SearchQuery = currentQuery;
        }

        ImGui.SameLine();

        bool isFilterActive = context.ShowFilters;
        if (isFilterActive) ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]);

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(filterIconText)) {
            context.ShowFilters = !context.ShowFilters;
            this.configurationService.Save();
        }
        ImGui.PopFont();

        if (isFilterActive) ImGui.PopStyleColor();

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("browser_tooltip_filters"));

        if (context.ShowFilters) {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.AlignTextToFramePadding();
            ImGui.Text(this.localization.Translate("browser_group_by"));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1f);

            string currentGroupLabel = context.CurrentGrouping switch {
                GroupingMode.NativeCategory => this.localization.Translate("browser_native_category"),
                GroupingMode.CustomGroup => this.localization.Translate("browser_custom_group"),
                _ => this.localization.Translate("browser_none")
            };

            if (ImGui.BeginCombo("##GroupingMode", currentGroupLabel)) {
                if (ImGui.Selectable(this.localization.Translate("browser_none"), context.CurrentGrouping == GroupingMode.None)) {
                    context.CurrentGrouping = GroupingMode.None;
                    filtersChanged = true;
                }
                if (ImGui.Selectable(this.localization.Translate("browser_native_category"), context.CurrentGrouping == GroupingMode.NativeCategory)) {
                    context.CurrentGrouping = GroupingMode.NativeCategory;
                    filtersChanged = true;
                }
                if (ImGui.Selectable(this.localization.Translate("browser_custom_group"), context.CurrentGrouping == GroupingMode.CustomGroup)) {
                    context.CurrentGrouping = GroupingMode.CustomGroup;
                    filtersChanged = true;
                }
                ImGui.EndCombo();
            }

            bool showModded = context.ShowModdedOnly;
            if (ImGui.Checkbox(this.localization.Translate("browser_show_modded"), ref showModded)) {
                context.ShowModdedOnly = showModded;
                filtersChanged = true;
            }

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(this.localization.Translate("browser_unlock_status"));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1f);

            string currentUnlockLabel = context.UnlockFilter switch {
                UnlockFilterMode.Unlocked => this.localization.Translate("browser_unlocked"),
                UnlockFilterMode.Locked => this.localization.Translate("browser_locked"),
                _ => this.localization.Translate("browser_all")
            };

            if (ImGui.BeginCombo("##UnlockStatus", currentUnlockLabel)) {
                if (ImGui.Selectable(this.localization.Translate("browser_all"), context.UnlockFilter == UnlockFilterMode.All)) {
                    context.UnlockFilter = UnlockFilterMode.All;
                    filtersChanged = true;
                }
                if (ImGui.Selectable(this.localization.Translate("browser_unlocked"), context.UnlockFilter == UnlockFilterMode.Unlocked)) {
                    context.UnlockFilter = UnlockFilterMode.Unlocked;
                    filtersChanged = true;
                }
                if (ImGui.Selectable(this.localization.Translate("browser_locked"), context.UnlockFilter == UnlockFilterMode.Locked)) {
                    context.UnlockFilter = UnlockFilterMode.Locked;
                    filtersChanged = true;
                }
                ImGui.EndCombo();
            }

            ImGui.Spacing();

            if (ImGui.BeginTable("FiltersLayoutTable", 3, ImGuiTableFlags.SizingStretchProp)) {
                ImGui.TableSetupColumn(this.localization.Translate("browser_categories"));
                ImGui.TableSetupColumn(this.localization.Translate("browser_groups"));
                ImGui.TableSetupColumn(this.localization.Translate("browser_tags"));
                ImGui.TableHeadersRow();

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                this.DrawMultiSelectCombo("##CatCombo", presenter.AvailableCategories, context.SelectedCategories, ref filtersChanged);

                ImGui.TableNextColumn();
                var groups = this.groupManagementService.GetGroups().Select(g => g.Name).ToList();
                this.DrawMultiSelectCombo("##GrpCombo", groups, context.SelectedGroups, ref filtersChanged);

                ImGui.TableNextColumn();
                var tags = this.tagManagementService.GetAvailableTags().ToList();
                this.DrawMultiSelectCombo("##TagCombo", tags, context.SelectedTags, ref filtersChanged);

                ImGui.EndTable();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }

        if (filtersChanged) {
            this.configurationService.Save();
            presenter.ApplyFilters();
        }
    }

    private void DrawMultiSelectCombo(string id, IReadOnlyList<string> items, HashSet<string> selectedItems, ref bool changed) {
        var preview = selectedItems.Count == 0 ? this.localization.Translate("browser_all") : $"{selectedItems.Count} selected";
        ImGui.SetNextItemWidth(-1f);

        if (ImGui.BeginCombo(id, preview)) {
            bool allSelected = selectedItems.Count == 0;
            if (ImGui.Checkbox(this.localization.Translate("browser_all"), ref allSelected)) {
                selectedItems.Clear();
                changed = true;
            }

            ImGui.Separator();

            foreach (var item in items) {
                bool isSelected = selectedItems.Contains(item);
                if (ImGui.Checkbox(item, ref isSelected)) {
                    if (isSelected) selectedItems.Add(item);
                    else selectedItems.Remove(item);

                    changed = true;
                }
            }
            ImGui.EndCombo();
        }
    }
}