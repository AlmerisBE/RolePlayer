namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class AllEmotesTab : IEmoteBrowserTab {
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IEmoteSelectionState selectionState;
    private IModStateProvider modStateProvider;
    private IEmoteExecutionService executionService;
    private ITextureProvider textureProvider;
    private IClientState clientState;
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;

    private List<EmoteDisplayData> emotesCache;
    private List<EmoteDisplayData> filteredEmotes;
    private List<string> availableCategories;

    private string searchQuery = string.Empty;
    private bool showFilters = false;
    private HashSet<string> selectedCategories = new();
    private HashSet<string> selectedGroups = new();
    private HashSet<string> selectedTags = new();

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
        ITagManagementService tagManagementService) {

        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.modStateProvider = modStateProvider;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.clientState = clientState;
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;

        this.emotesCache = new List<EmoteDisplayData>();
        this.filteredEmotes = new List<EmoteDisplayData>();
        this.availableCategories = new List<string>();
    }

    public void Draw() {
        if (!this.emotesCache.Any()) {
            this.LoadEmotes();
        }

        bool filtersChanged = false;

        var filterBtnText = "Filters";
        var filterBtnWidth = ImGui.CalcTextSize(filterBtnText).X + ImGui.GetStyle().FramePadding.X * 2;

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - filterBtnWidth - ImGui.GetStyle().ItemSpacing.X);
        if (ImGui.InputTextWithHint("##SearchEmotes", "Search by name or command...", ref this.searchQuery, 128)) {
            filtersChanged = true;
        }

        ImGui.SameLine();
        if (ImGui.Button(filterBtnText)) {
            this.showFilters = !this.showFilters;
        }

        if (this.showFilters) {
            if (ImGui.BeginTable("FiltersTable", 3)) {
                ImGui.TableSetupColumn("Categories");
                ImGui.TableSetupColumn("Groups");
                ImGui.TableSetupColumn("Tags");
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                this.DrawMultiSelectCombo("Categories##Combo", this.availableCategories, this.selectedCategories, ref filtersChanged);

                ImGui.TableNextColumn();
                var groups = this.groupManagementService.GetGroups().Select(g => g.Name).ToList();
                this.DrawMultiSelectCombo("Groups##Combo", groups, this.selectedGroups, ref filtersChanged);

                ImGui.TableNextColumn();
                var tags = this.tagManagementService.GetAvailableTags().ToList();
                this.DrawMultiSelectCombo("Tags##Combo", tags, this.selectedTags, ref filtersChanged);

                ImGui.EndTable();
            }
            ImGui.Spacing();
        }

        if (filtersChanged) {
            this.ApplyFilters();
        }

        var tableFlags = ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;

        if (ImGui.BeginTable("AllEmotesTable", 4, tableFlags)) {
            ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 32f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableHeadersRow();

            foreach (var emote in this.filteredEmotes) {
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
                        var lookup = new GameIconLookup {
                            IconId = emote.IconId,
                            HiRes = false
                        };

                        var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();
                        if (iconWrap != null) {
                            ImGui.Image(iconWrap.Handle, new Vector2(24, 24));
                        }
                    }
                    catch (IconNotFoundException) {
                        // Silently ignore missing textures
                    }
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
            ImGui.EndTable();
        }
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

    private void ApplyFilters() {
        this.filteredEmotes.Clear();

        var query = this.searchQuery.Trim().ToLowerInvariant();
        bool hasSearch = !string.IsNullOrEmpty(query);
        bool hasCatFilter = this.selectedCategories.Count > 0;
        bool hasGroupFilter = this.selectedGroups.Count > 0;
        bool hasTagFilter = this.selectedTags.Count > 0;

        foreach (var emote in this.emotesCache) {
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

            if (hasGroupFilter) {
                var group = this.groupManagementService.GetGroupForEmote(emote.Id);
                if (group == null || !this.selectedGroups.Contains(group)) {
                    continue;
                }
            }

            if (hasTagFilter) {
                var tags = this.tagManagementService.GetTagsForEmote(emote.Id);
                if (!this.selectedTags.Overlaps(tags)) {
                    continue;
                }
            }

            this.filteredEmotes.Add(emote);
        }
    }

    private void LoadEmotes() {
        var baseEmotes = this.emoteRepository.GetBaseEmotes();
        var uniqueCategories = new HashSet<string>();

        foreach (var emote in baseEmotes) {
            emote.IsUnlocked = !emote.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(emote.Id);
            var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
            emote.IsModded = !string.IsNullOrEmpty(modName);

            this.emotesCache.Add(emote);
            if (!string.IsNullOrEmpty(emote.Category)) {
                uniqueCategories.Add(emote.Category);
            }
        }

        this.availableCategories = uniqueCategories.OrderBy(c => c).ToList();
        this.ApplyFilters();
    }
}