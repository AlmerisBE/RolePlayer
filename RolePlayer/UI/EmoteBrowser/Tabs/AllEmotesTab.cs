namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public class AllEmotesTab : IEmoteBrowserTab, IDisposable {
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IEmoteSelectionState selectionState;
    private IModStateProvider modStateProvider;
    private IEmoteExecutionService executionService;
    private ITextureProvider textureProvider;
    private IClientState clientState;
    private ILoggerService logger;
    private IConfigurationService configurationService;
    private IContextManagementService contextService;
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;
    private HotbarManagerComponent hotbarManager;
    private EmoteFilterComponent filterComponent;
    private EmoteDetailsPanel detailsPanel;
    private ILocalizationService localization;

    private List<EmoteDisplayData> emotesCache = new();
    private List<string> availableCategories = new();
    private Dictionary<string, List<EmoteDisplayData>> groupedEmotes = new();

    private bool needsRefresh = false;
    private bool needsFilterApply = false;
    private bool isRefreshing = false;

    public string TabName => this.localization.Translate("browser_tab_all_emotes");
    public int SortOrder => 0;
    public bool IsSidePanelOpen => this.selectionState.SelectedEmote != null;

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IEmoteSelectionState selectionState,
        IModStateProvider modStateProvider,
        IEmoteExecutionService executionService,
        ITextureProvider textureProvider,
        IClientState clientState,
        ILoggerService logger,
        IConfigurationService configurationService,
        IContextManagementService contextService,
        IGroupManagementService groupManagementService,
        ITagManagementService tagManagementService,
        HotbarManagerComponent hotbarManager,
        EmoteFilterComponent filterComponent,
        EmoteDetailsPanel detailsPanel,
        ILocalizationService localization) {

        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.modStateProvider = modStateProvider;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.clientState = clientState;
        this.logger = logger;
        this.configurationService = configurationService;
        this.contextService = contextService;
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
        this.hotbarManager = hotbarManager;
        this.filterComponent = filterComponent;
        this.detailsPanel = detailsPanel;
        this.localization = localization;

        this.modStateProvider.ModStateChanged += this.OnModStateChanged;
        this.playerStateProvider.PlayerStateValid += this.OnPlayerStateValid;
    }

    private void OnModStateChanged() => this.needsRefresh = true;
    private void OnPlayerStateValid() => this.needsRefresh = true;

    public void Draw() {
        if (this.needsRefresh && !this.isRefreshing) {
            this.needsRefresh = false;
            this.LoadEmotesAsync();
        }

        if (!this.emotesCache.Any() && !this.isRefreshing) this.LoadEmotesAsync();

        bool filtersChanged = this.filterComponent.Draw(this.availableCategories);

        if (filtersChanged || this.needsFilterApply) {
            this.groupedEmotes = this.filterComponent.Apply(this.emotesCache);
            this.needsFilterApply = false;
        }

        if (ImGui.BeginChild("EmoteListScrollArea", new Vector2(0, 0), false, ImGuiWindowFlags.None)) {
            var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingFixedFit;
            var context = this.contextService.GetCurrentContext();

            foreach (var groupKvp in this.groupedEmotes.OrderBy(k => k.Key)) {
                bool isNodeOpen = true;

                if (context.CurrentGrouping != GroupingMode.None) isNodeOpen = ImGui.CollapsingHeader($"{groupKvp.Key} ({groupKvp.Value.Count})###Header_{groupKvp.Key}", ImGuiTreeNodeFlags.DefaultOpen);

                if (isNodeOpen) {
                    if (ImGui.BeginTable($"AllEmotesTable_{groupKvp.Key}", 4, tableFlags)) {
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_icon"), ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 32f);
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_name"), ImGuiTableColumnFlags.WidthStretch, 0.4f);
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_command"), ImGuiTableColumnFlags.WidthStretch, 0.4f);
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_action"), ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 40f);
                        ImGui.TableHeadersRow();

                        var sortSpecs = ImGui.TableGetSortSpecs();
                        if (sortSpecs.SpecsDirty) {
                            this.filterComponent.RegisterSort(sortSpecs.Specs.ColumnIndex, sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending);
                            this.groupedEmotes = this.filterComponent.Apply(this.emotesCache);
                            sortSpecs.SpecsDirty = false;
                        }

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
                            if (ImGui.Selectable($"##select_{emote.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) this.selectionState.SelectedEmote = isSelected ? null : emote;

                            this.DrawContextMenu(emote, context);

                            ImGui.SameLine();

                            if (emote.IconId > 0) {
                                try {
                                    var lookup = new GameIconLookup { IconId = emote.IconId, HiRes = false };
                                    var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

                                    if (iconWrap != null) {
                                        var cursorPos = ImGui.GetCursorScreenPos();
                                        ImGui.Image(iconWrap.Handle, new Vector2(24, 24));

                                        if (emote.HasVariations) {
                                            var drawList = ImGui.GetWindowDrawList();
                                            ImGui.PushFont(UiBuilder.IconFont);
                                            var indicatorText = FontAwesomeIcon.Sync.ToIconString();
                                            var textSize = ImGui.CalcTextSize(indicatorText);
                                            ImGui.PopFont();

                                            var indicatorPos = new Vector2(cursorPos.X + 24f - textSize.X - 1f, cursorPos.Y + 24f - textSize.Y - 1f);
                                            drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), new Vector2(indicatorPos.X + 1, indicatorPos.Y + 1), 0xFF000000, indicatorText);
                                            drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), indicatorPos, 0xFF40DD40, indicatorText);
                                        }
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

                            if (this.clientState.ClientLanguage != ClientLanguage.English && !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand != emote.LocalizedCommand) commandText += $" / {emote.EnglishCommand}";

                            ImGui.Text(commandText);

                            ImGui.TableNextColumn();
                            if (emote.IsUnlocked) {
                                ImGui.PushFont(UiBuilder.IconFont);
                                if (ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##{emote.Id}", new Vector2(-1, 24))) this.executionService.ExecuteEmote(emote.Id);
                                ImGui.PopFont();

                                if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("browser_ctx_execute"));
                            }

                            if (hasCustomColor) ImGui.PopStyleColor();
                        }
                        ImGui.EndTable();
                    }
                }
            }
        }
        ImGui.EndChild();
    }

    private void DrawContextMenu(EmoteDisplayData emote, EmoteContext context) {
        if (ImGui.BeginPopupContextItem($"EmoteContextMenu_{emote.Id}")) {
            if (ImGui.MenuItem(this.localization.Translate("browser_ctx_copy"))) ImGui.SetClipboardText(emote.LocalizedCommand);

            if (ImGui.MenuItem(this.localization.Translate("browser_ctx_execute"), "", false, emote.IsUnlocked)) this.executionService.ExecuteEmote(emote.Id);

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

                if (groupChanged) this.needsFilterApply = true;
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

                if (tagChanged) this.needsFilterApply = true;
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private void LoadEmotesAsync() {
        if (!this.playerStateProvider.IsPlayerValid) return;

        this.isRefreshing = true;
        Task.Run(() => {
            try {
                var baseEmotes = this.emoteRepository.GetBaseEmotes().ToList();
                var uniqueCategories = new HashSet<string>();
                var newCache = new List<EmoteDisplayData>();

                foreach (var emote in baseEmotes) {
                    emote.IsUnlocked = !emote.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(emote.Id);
                    emote.IsModded = !string.IsNullOrEmpty(this.modStateProvider.GetModNameModifyingEmote(emote.Id));

                    newCache.Add(emote);
                    if (!string.IsNullOrEmpty(emote.Category)) uniqueCategories.Add(emote.Category);
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

    public void DrawSidePanel() => this.detailsPanel.Draw();

    public void Dispose() {
        this.modStateProvider.ModStateChanged -= this.OnModStateChanged;
        this.playerStateProvider.PlayerStateValid -= this.OnPlayerStateValid;
    }
}