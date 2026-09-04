namespace RolePlayer.UI.EmoteBrowser.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;
using System.Linq;

public class EmoteFilterComponent {
    private IConfigurationService configurationService;
    private IContextManagementService contextService;
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;

    public string SearchQuery = string.Empty;
    private int sortColumn = -1;
    private bool sortDescending = false;

    public EmoteFilterComponent(
        IConfigurationService configurationService,
        IContextManagementService contextService,
        IGroupManagementService groupManagementService,
        ITagManagementService tagManagementService) {

        this.configurationService = configurationService;
        this.contextService = contextService;
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
    }

    public bool Draw(List<string> availableCategories) {
        bool filtersChanged = false;
        var context = this.contextService.GetCurrentContext();

        ImGui.PushFont(UiBuilder.IconFont);
        var filterIconText = FontAwesomeIcon.Filter.ToIconString();
        var filterIconWidth = ImGui.CalcTextSize(filterIconText).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - filterIconWidth - ImGui.GetStyle().ItemSpacing.X);
        if (ImGui.InputTextWithHint("##SearchEmotes", "Search by name or command...", ref this.SearchQuery, 128)) {
            filtersChanged = true;
        }

        ImGui.SameLine();

        bool isFilterActive = context.ShowFilters;

        if (isFilterActive) {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]);
        }

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(filterIconText)) {
            context.ShowFilters = !context.ShowFilters;
            this.configurationService.Save();
        }
        ImGui.PopFont();

        if (isFilterActive) {
            ImGui.PopStyleColor();
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Toggle Advanced Filters");
        }

        if (context.ShowFilters) {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Group By:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150f);

            string currentGroupLabel = context.CurrentGrouping switch {
                GroupingMode.NativeCategory => "Native Category",
                GroupingMode.CustomGroup => "Custom Group",
                _ => "None"
            };

            if (ImGui.BeginCombo("##GroupingMode", currentGroupLabel)) {
                if (ImGui.Selectable("None", context.CurrentGrouping == GroupingMode.None)) {
                    context.CurrentGrouping = GroupingMode.None;
                    this.configurationService.Save();
                    filtersChanged = true;
                }
                if (ImGui.Selectable("Native Category", context.CurrentGrouping == GroupingMode.NativeCategory)) {
                    context.CurrentGrouping = GroupingMode.NativeCategory;
                    this.configurationService.Save();
                    filtersChanged = true;
                }
                if (ImGui.Selectable("Custom Group", context.CurrentGrouping == GroupingMode.CustomGroup)) {
                    context.CurrentGrouping = GroupingMode.CustomGroup;
                    this.configurationService.Save();
                    filtersChanged = true;
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            bool showModded = context.ShowModdedOnly;
            if (ImGui.Checkbox("Show Modded Only", ref showModded)) {
                context.ShowModdedOnly = showModded;
                this.configurationService.Save();
                filtersChanged = true;
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(120f);
            if (ImGui.BeginCombo("Unlock Status", context.UnlockFilter.ToString())) {
                if (ImGui.Selectable("All", context.UnlockFilter == UnlockFilterMode.All)) {
                    context.UnlockFilter = UnlockFilterMode.All;
                    this.configurationService.Save();
                    filtersChanged = true;
                }
                if (ImGui.Selectable("Unlocked", context.UnlockFilter == UnlockFilterMode.Unlocked)) {
                    context.UnlockFilter = UnlockFilterMode.Unlocked;
                    this.configurationService.Save();
                    filtersChanged = true;
                }
                if (ImGui.Selectable("Locked", context.UnlockFilter == UnlockFilterMode.Locked)) {
                    context.UnlockFilter = UnlockFilterMode.Locked;
                    this.configurationService.Save();
                    filtersChanged = true;
                }
                ImGui.EndCombo();
            }

            ImGui.Spacing();

            if (ImGui.BeginTable("FiltersLayoutTable", 3)) {
                ImGui.TableSetupColumn("Categories");
                ImGui.TableSetupColumn("Groups");
                ImGui.TableSetupColumn("Tags");
                ImGui.TableNextRow();

                bool multiSelectChanged = false;

                ImGui.TableNextColumn();
                this.DrawMultiSelectCombo("Categories##Combo", availableCategories, context.SelectedCategories, ref multiSelectChanged);

                ImGui.TableNextColumn();
                var groups = this.groupManagementService.GetGroups().Select(g => g.Name).ToList();
                this.DrawMultiSelectCombo("Groups##Combo", groups, context.SelectedGroups, ref multiSelectChanged);

                ImGui.TableNextColumn();
                var tags = this.tagManagementService.GetAvailableTags().ToList();
                this.DrawMultiSelectCombo("Tags##Combo", tags, context.SelectedTags, ref multiSelectChanged);

                if (multiSelectChanged) {
                    this.configurationService.Save();
                    filtersChanged = true;
                }

                ImGui.EndTable();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }

        return filtersChanged;
    }

    private void DrawMultiSelectCombo(string label, List<string> items, HashSet<string> selectedItems, ref bool changed) {
        var preview = selectedItems.Count == 0 ? "All" : $"{selectedItems.Count} selected";
        ImGui.SetNextItemWidth(-1f);

        if (ImGui.BeginCombo(label, preview)) {
            bool allSelected = selectedItems.Count == 0;
            if (ImGui.Checkbox("All", ref allSelected)) {
                selectedItems.Clear();
                changed = true;
            }

            ImGui.Separator();

            foreach (var item in items) {
                bool isSelected = selectedItems.Contains(item);
                if (ImGui.Checkbox(item, ref isSelected)) {
                    if (isSelected) {
                        selectedItems.Add(item);
                    }
                    else {
                        selectedItems.Remove(item);
                    }

                    changed = true;
                }
            }
            ImGui.EndCombo();
        }
    }

    public void RegisterSort(int columnIndex, bool descending) {
        this.sortColumn = columnIndex;
        this.sortDescending = descending;
    }

    public Dictionary<string, List<EmoteDisplayData>> Apply(List<EmoteDisplayData> emotesCache) {
        var groupedEmotes = new Dictionary<string, List<EmoteDisplayData>>();
        var context = this.contextService.GetCurrentContext();

        var query = this.SearchQuery.Trim().ToLowerInvariant();
        bool hasSearch = !string.IsNullOrEmpty(query);
        bool hasCatFilter = context.SelectedCategories.Count > 0;
        bool hasGroupFilter = context.SelectedGroups.Count > 0;
        bool hasTagFilter = context.SelectedTags.Count > 0;

        foreach (var emote in emotesCache) {
            if (context.ShowModdedOnly && !emote.IsModded) {
                continue;
            }

            if (context.UnlockFilter == UnlockFilterMode.Unlocked && !emote.IsUnlocked) {
                continue;
            }

            if (context.UnlockFilter == UnlockFilterMode.Locked && emote.IsUnlocked) {
                continue;
            }

            if (hasSearch) {
                bool matchesName = emote.Name.ToLowerInvariant().Contains(query);
                bool matchesCmd = emote.LocalizedCommand.ToLowerInvariant().Contains(query);
                bool matchesEnCmd = !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand.ToLowerInvariant().Contains(query);
                if (!matchesName && !matchesCmd && !matchesEnCmd) {
                    continue;
                }
            }

            if (hasCatFilter && !context.SelectedCategories.Contains(emote.Category)) {
                continue;
            }

            var customGroup = this.groupManagementService.GetGroupForEmote(emote.Id);
            if (hasGroupFilter && (string.IsNullOrEmpty(customGroup) || !context.SelectedGroups.Contains(customGroup))) {
                continue;
            }

            if (hasTagFilter) {
                var tags = this.tagManagementService.GetTagsForEmote(emote.Id);
                if (!context.SelectedTags.Overlaps(tags)) {
                    continue;
                }
            }

            string groupKey = "All";
            if (context.CurrentGrouping == GroupingMode.NativeCategory) {
                groupKey = string.IsNullOrEmpty(emote.Category) ? "Uncategorized" : emote.Category;
            }
            else if (context.CurrentGrouping == GroupingMode.CustomGroup) {
                groupKey = string.IsNullOrEmpty(customGroup) ? "Ungrouped" : customGroup;
            }

            if (!groupedEmotes.ContainsKey(groupKey)) {
                groupedEmotes[groupKey] = new List<EmoteDisplayData>();
            }

            groupedEmotes[groupKey].Add(emote);
        }

        if (this.sortColumn == -1) {
            return groupedEmotes;
        }

        foreach (var key in groupedEmotes.Keys.ToList()) {
            var list = groupedEmotes[key];

            if (this.sortColumn == 1) {
                list = this.sortDescending ? list.OrderByDescending(e => e.Name).ToList() : list.OrderBy(e => e.Name).ToList();
            }
            else if (this.sortColumn == 2) {
                list = this.sortDescending ? list.OrderByDescending(e => e.LocalizedCommand).ToList() : list.OrderBy(e => e.LocalizedCommand).ToList();
            }

            groupedEmotes[key] = list;
        }

        return groupedEmotes;
    }
}