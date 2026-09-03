namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public enum GroupingMode {
    None,
    NativeCategory,
    CustomGroup
}

public class AllEmotesTab : IEmoteBrowserTab, IDisposable {
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IEmoteSelectionState selectionState;
    private IModStateProvider modStateProvider;
    private IEmoteExecutionService executionService;
    private ITextureProvider textureProvider;
    private IClientState clientState;
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;
    private ILoggerService logger;

    private List<EmoteDisplayData> emotesCache;
    private List<string> availableCategories;
    private Dictionary<string, List<EmoteDisplayData>> groupedEmotes;

    private string searchQuery = string.Empty;
    private bool showModdedOnly = false;
    private GroupingMode currentGrouping = GroupingMode.NativeCategory;

    private HashSet<string> selectedCategories = new();
    private HashSet<string> selectedGroups = new();
    private HashSet<string> selectedTags = new();

    private bool needsRefresh = false;
    private bool needsFilterApply = false;
    private bool isRefreshing = false;

    private int sortColumn = -1;
    private bool sortDescending = false;

    public string TabName => "All Emotes";
    public int SortOrder => 0;

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IEmoteSelectionState selectionState,
        IModStateProvider modStateProvider,
        IEmoteExecutionService executionService,
        ITextureProvider textureProvider,
        IClientState clientState,
        IGroupManagementService groupManagementService,
        ITagManagementService tagManagementService,
        ILoggerService logger) {

        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.modStateProvider = modStateProvider;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.clientState = clientState;
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
        this.logger = logger;

        this.emotesCache = new List<EmoteDisplayData>();
        this.availableCategories = new List<string>();
        this.groupedEmotes = new Dictionary<string, List<EmoteDisplayData>>();

        this.modStateProvider.ModStateChanged += this.OnModStateChanged;
    }

    private void OnModStateChanged() {
        this.needsRefresh = true;
    }

    public void Draw() {
        if (this.needsRefresh && !this.isRefreshing) {
            this.needsRefresh = false;
            this.LoadEmotesAsync();
        }

        if (this.needsFilterApply) {
            this.ApplyFilters();
            this.needsFilterApply = false;
        }

        if (!this.emotesCache.Any() && !this.isRefreshing) {
            this.LoadEmotesAsync();
        }

        bool filtersChanged = false;

        ImGui.PushFont(UiBuilder.IconFont);
        var syncIconWidth = ImGui.CalcTextSize(FontAwesomeIcon.Sync.ToIconString()).X + ImGui.GetStyle().FramePadding.X * 2;
        var filterIconWidth = ImGui.CalcTextSize(FontAwesomeIcon.Filter.ToIconString()).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - syncIconWidth - filterIconWidth - (ImGui.GetStyle().ItemSpacing.X * 2));
        if (ImGui.InputTextWithHint("##SearchEmotes", "Search by name or command...", ref this.searchQuery, 128)) {
            filtersChanged = true;
        }

        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (this.isRefreshing) {
            ImGui.BeginDisabled();
            ImGui.Button(FontAwesomeIcon.Sync.ToIconString());
            ImGui.EndDisabled();
        }
        else {
            if (ImGui.Button(FontAwesomeIcon.Sync.ToIconString())) {
                this.needsRefresh = true;
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Force synchronization with Penumbra");
            }
        }

        ImGui.SameLine();
        if (ImGui.Button(FontAwesomeIcon.Filter.ToIconString())) {
            ImGui.OpenPopup("AdvancedFiltersPopup");
        }

        ImGui.PopFont();
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Advanced Filters");
        }

        if (ImGui.BeginPopup("AdvancedFiltersPopup")) {
            ImGui.TextDisabled("Filters");
            ImGui.Separator();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Categories:");
            ImGui.SameLine(80f);
            this.DrawMultiSelectCombo("##CatCombo", this.availableCategories, this.selectedCategories, ref filtersChanged);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Groups:");
            ImGui.SameLine(80f);
            var groups = this.groupManagementService.GetGroups().Select(g => g.Name).ToList();
            this.DrawMultiSelectCombo("##GrpCombo", groups, this.selectedGroups, ref filtersChanged);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Tags:");
            ImGui.SameLine(80f);
            var tags = this.tagManagementService.GetAvailableTags().ToList();
            this.DrawMultiSelectCombo("##TagCombo", tags, this.selectedTags, ref filtersChanged);

            ImGui.Separator();
            if (ImGui.Checkbox("Show Modded Only", ref this.showModdedOnly)) {
                filtersChanged = true;
            }

            ImGui.EndPopup();
        }

        ImGui.Spacing();
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Group By:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);

        string currentGroupLabel = this.currentGrouping switch {
            GroupingMode.NativeCategory => "Native Category",
            GroupingMode.CustomGroup => "Custom Group",
            _ => "None"
        };

        if (ImGui.BeginCombo("##GroupingMode", currentGroupLabel)) {
            if (ImGui.Selectable("None", this.currentGrouping == GroupingMode.None)) { this.currentGrouping = GroupingMode.None; filtersChanged = true; }
            if (ImGui.Selectable("Native Category", this.currentGrouping == GroupingMode.NativeCategory)) { this.currentGrouping = GroupingMode.NativeCategory; filtersChanged = true; }
            if (ImGui.Selectable("Custom Group", this.currentGrouping == GroupingMode.CustomGroup)) { this.currentGrouping = GroupingMode.CustomGroup; filtersChanged = true; }
            ImGui.EndCombo();
        }

        if (filtersChanged) {
            this.ApplyFilters();
        }

        // Ajout de ImGuiTableFlags.Borders pour reproduire le style fermé de Befriender et ImGuiTableFlags.Sortable pour les colonnes
        var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable("AllEmotesTable", 4, tableFlags)) {
            ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 32f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 60f);
            ImGui.TableHeadersRow();

            var sortSpecs = ImGui.TableGetSortSpecs();
            if (sortSpecs.SpecsDirty) {
                this.sortColumn = sortSpecs.Specs.ColumnIndex;
                this.sortDescending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending;
                this.ApplyCurrentSorting();
                sortSpecs.SpecsDirty = false;
            }

            foreach (var groupKvp in this.groupedEmotes) {
                bool isNodeOpen = true;

                if (this.currentGrouping != GroupingMode.None) {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    var headerLabel = $"{groupKvp.Key} ({groupKvp.Value.Count})";
                    isNodeOpen = ImGui.TreeNodeEx(headerLabel, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                }

                if (isNodeOpen) {
                    foreach (var emote in groupKvp.Value) {
                        ImGui.TableNextRow();

                        var isSelected = this.selectionState.SelectedEmote?.Id == emote.Id;
                        var hasCustomColor = false;

                        if (!emote.IsUnlocked) {
                            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF808080);
                            hasCustomColor = true;
                        }
                        else if (emote.IsModded) {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                            hasCustomColor = true;
                        }

                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"##select_{emote.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
                            this.selectionState.SelectedEmote = isSelected ? null : emote;
                        }

                        ImGui.SameLine();

                        if (emote.IconId > 0) {
                            try {
                                var lookup = new GameIconLookup { IconId = emote.IconId, HiRes = false };
                                var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();
                                if (iconWrap != null) {
                                    ImGui.Image(iconWrap.Handle, new Vector2(24, 24));
                                }
                            }
                            catch (IconNotFoundException) { }
                        }

                        ImGui.TableNextColumn();
                        var displayName = emote.IsModded ? $"★ {emote.Name}" : emote.Name;
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text(displayName);

                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();
                        var commandText = emote.LocalizedCommand;

                        if (this.clientState.ClientLanguage != ClientLanguage.English && !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand != emote.LocalizedCommand) {
                            commandText += $" / {emote.EnglishCommand}";
                        }

                        ImGui.Text(commandText);

                        ImGui.TableNextColumn();
                        if (emote.IsUnlocked) {
                            if (ImGui.Button($"Play##{emote.Id}", new Vector2(-1, 24))) {
                                this.executionService.ExecuteEmote(emote.Id);
                            }
                        }

                        if (hasCustomColor) {
                            ImGui.PopStyleColor();
                        }
                    }

                    if (this.currentGrouping != GroupingMode.None) {
                        ImGui.TreePop();
                    }
                }
            }
            ImGui.EndTable();
        }
    }

    private void DrawMultiSelectCombo(string label, List<string> items, HashSet<string> selectedItems, ref bool changed) {
        var preview = selectedItems.Count == 0 ? "All" : $"{selectedItems.Count} selected";
        ImGui.SetNextItemWidth(150f);

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

    private void ApplyFilters() {
        this.groupedEmotes.Clear();

        var query = this.searchQuery.Trim().ToLowerInvariant();
        bool hasSearch = !string.IsNullOrEmpty(query);
        bool hasCatFilter = this.selectedCategories.Count > 0;
        bool hasGroupFilter = this.selectedGroups.Count > 0;
        bool hasTagFilter = this.selectedTags.Count > 0;

        foreach (var emote in this.emotesCache) {
            if (this.showModdedOnly && !emote.IsModded) {
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

            if (hasCatFilter && !this.selectedCategories.Contains(emote.Category)) {
                continue;
            }

            var customGroup = this.groupManagementService.GetGroupForEmote(emote.Id);
            if (hasGroupFilter && (string.IsNullOrEmpty(customGroup) || !this.selectedGroups.Contains(customGroup))) {
                continue;
            }

            if (hasTagFilter) {
                var tags = this.tagManagementService.GetTagsForEmote(emote.Id);
                if (!this.selectedTags.Overlaps(tags)) {
                    continue;
                }
            }

            string groupKey = "All";
            if (this.currentGrouping == GroupingMode.NativeCategory) {
                groupKey = string.IsNullOrEmpty(emote.Category) ? "Uncategorized" : emote.Category;
            }
            else if (this.currentGrouping == GroupingMode.CustomGroup) {
                groupKey = string.IsNullOrEmpty(customGroup) ? "Ungrouped" : customGroup;
            }

            if (!this.groupedEmotes.ContainsKey(groupKey)) {
                this.groupedEmotes[groupKey] = new List<EmoteDisplayData>();
            }

            this.groupedEmotes[groupKey].Add(emote);
        }

        this.ApplyCurrentSorting();
    }

    private void ApplyCurrentSorting() {
        if (this.sortColumn == -1) {
            return;
        }

        foreach (var key in this.groupedEmotes.Keys.ToList()) {
            var list = this.groupedEmotes[key];
            if (this.sortColumn == 1) {
                list = this.sortDescending ? list.OrderByDescending(e => e.Name).ToList() : list.OrderBy(e => e.Name).ToList();
            }
            else if (this.sortColumn == 2) {
                list = this.sortDescending ? list.OrderByDescending(e => e.LocalizedCommand).ToList() : list.OrderBy(e => e.LocalizedCommand).ToList();
            }

            this.groupedEmotes[key] = list;
        }
    }

    private void LoadEmotesAsync() {
        if (this.isRefreshing) {
            return;
        }

        this.isRefreshing = true;

        Task.Run(() => {
            try {
                var baseEmotes = this.emoteRepository.GetBaseEmotes().ToList();
                var uniqueCategories = new HashSet<string>();
                var newCache = new List<EmoteDisplayData>();

                foreach (var emote in baseEmotes) {
                    emote.IsUnlocked = !emote.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(emote.Id);
                    var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
                    emote.IsModded = !string.IsNullOrEmpty(modName);

                    newCache.Add(emote);
                    if (!string.IsNullOrEmpty(emote.Category)) {
                        uniqueCategories.Add(emote.Category);
                    }
                }

                this.emotesCache = newCache;
                this.availableCategories = uniqueCategories.OrderBy(c => c).ToList();
                this.needsFilterApply = true;
            }
            catch (Exception ex) {
                this.logger.Error(ex, "[AllEmotesTab] Background emote resolution failed unexpectedly.");
            }
            finally {
                this.isRefreshing = false;
            }
        });
    }

    public void Dispose() {
        this.modStateProvider.ModStateChanged -= this.OnModStateChanged;
    }
}