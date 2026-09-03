namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
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

    private EmoteFilterComponent filterComponent;

    private List<EmoteDisplayData> emotesCache = new();
    private List<string> availableCategories = new();
    private Dictionary<string, List<EmoteDisplayData>> groupedEmotes = new();

    private bool needsRefresh = false;
    private bool needsFilterApply = false;
    private bool isRefreshing = false;

    public string TabName => "All Emotes";
    public int SortOrder => 0;
    public bool SupportsSidePanel => true;

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IEmoteSelectionState selectionState,
        IModStateProvider modStateProvider,
        IEmoteExecutionService executionService,
        ITextureProvider textureProvider,
        IClientState clientState,
        ILoggerService logger,
        EmoteFilterComponent filterComponent) {

        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.modStateProvider = modStateProvider;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.clientState = clientState;
        this.logger = logger;
        this.filterComponent = filterComponent;

        this.modStateProvider.ModStateChanged += this.OnModStateChanged;
    }

    private void OnModStateChanged() => this.needsRefresh = true;

    public void Draw() {
        if (this.needsRefresh && !this.isRefreshing) {
            this.needsRefresh = false;
            this.LoadEmotesAsync();
        }

        if (!this.emotesCache.Any() && !this.isRefreshing) {
            this.LoadEmotesAsync();
        }

        bool filtersChanged = this.filterComponent.Draw(this.availableCategories);

        if (filtersChanged || this.needsFilterApply) {
            this.groupedEmotes = this.filterComponent.Apply(this.emotesCache);
            this.needsFilterApply = false;
        }

        if (ImGui.BeginChild("EmoteListScrollArea", new Vector2(0, 0), false, ImGuiWindowFlags.None)) {
            var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingFixedFit;

            foreach (var groupKvp in this.groupedEmotes.OrderBy(k => k.Key)) {
                if (this.filterComponent.CurrentGrouping != GroupingMode.None) {
                    ImGui.Spacing();
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), $"▼ {groupKvp.Key} ({groupKvp.Value.Count})");
                    ImGui.Separator();
                }

                if (ImGui.BeginTable($"AllEmotesTable_{groupKvp.Key}", 4, tableFlags)) {
                    ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 32f);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.4f);
                    ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch, 0.4f);
                    // Réduction de la largeur de la colonne d'action pour s'ajuster à l'icône
                    ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 40f);
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
                            ImGui.PushFont(UiBuilder.IconFont);
                            if (ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##{emote.Id}", new Vector2(-1, 24))) {
                                this.executionService.ExecuteEmote(emote.Id);
                            }

                            ImGui.PopFont();

                            if (ImGui.IsItemHovered()) {
                                ImGui.SetTooltip("Execute Emote");
                            }
                        }

                        if (hasCustomColor) {
                            ImGui.PopStyleColor();
                        }
                    }
                    ImGui.EndTable();
                }
            }
        }
        ImGui.EndChild();
    }

    private void LoadEmotesAsync() {
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

    public void Dispose() => this.modStateProvider.ModStateChanged -= this.OnModStateChanged;
}