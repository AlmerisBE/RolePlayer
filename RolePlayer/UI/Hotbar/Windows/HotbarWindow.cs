namespace RolePlayer.UI.Hotbar.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class HotbarWindow : Window {
    private HotbarConfig config;
    private IHotbarResolverService resolverService;
    private IEmoteExecutionService executionService;
    private ITextureProvider textureProvider;
    private Func<IEnumerable<EmoteDisplayData>> emoteCacheProvider;

    private int currentPage = 0;
    private const int MaxItemsPerPage = 16;
    private const float IconSize = 40f;

    public HotbarWindow(
        HotbarConfig config,
        IHotbarResolverService resolverService,
        IEmoteExecutionService executionService,
        ITextureProvider textureProvider,
        Func<IEnumerable<EmoteDisplayData>> emoteCacheProvider)
        : base($"RolePlayer_Hotbar_{config.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize) {

        this.config = config;
        this.resolverService = resolverService;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.emoteCacheProvider = emoteCacheProvider;

        this.SizeCondition = ImGuiCond.Always;
        this.BgAlpha = 0.7f;
    }

    public override void Draw() {
        if (!this.config.IsVisible) {
            return;
        }

        var allEmotes = this.emoteCacheProvider();
        var resolvedEmotes = this.resolverService.ResolveEmotesForHotbar(this.config, allEmotes);

        int totalPages = (int)Math.Ceiling(resolvedEmotes.Count / (double)MaxItemsPerPage);
        if (this.currentPage >= totalPages && totalPages > 0) {
            this.currentPage = totalPages - 1;
        }

        if (totalPages == 0) {
            this.currentPage = 0;
        }

        var displayedEmotes = resolvedEmotes.Skip(this.currentPage * MaxItemsPerPage).Take(MaxItemsPerPage).ToList();

        int columns = this.GetColumnsForLayout(this.config.Layout);

        if (ImGui.BeginTable($"HotbarGrid_{this.config.Id}", columns, ImGuiTableFlags.SizingFixedFit)) {
            for (int i = 0; i < MaxItemsPerPage; i++) {
                if (i % columns == 0) {
                    ImGui.TableNextRow();
                }

                ImGui.TableNextColumn();

                if (i < displayedEmotes.Count) {
                    var emote = displayedEmotes[i];
                    this.DrawEmoteIcon(emote);
                }
                else {
                    this.DrawEmptySlot();
                }
            }
            ImGui.EndTable();
        }

        if (totalPages > 1) {
            this.DrawPagination(totalPages);
        }
    }

    private int GetColumnsForLayout(HotbarLayout layout) {
        return layout switch {
            HotbarLayout.Grid16x1 => 16,
            HotbarLayout.Grid8x2 => 8,
            HotbarLayout.Grid4x4 => 4,
            HotbarLayout.Grid2x8 => 2,
            HotbarLayout.Grid1x16 => 1,
            _ => 16
        };
    }

    private void DrawEmoteIcon(EmoteDisplayData emote) {
        bool drawn = false;

        if (emote.IconId > 0) {
            try {
                var lookup = new GameIconLookup { IconId = emote.IconId, HiRes = false };
                var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

                if (iconWrap != null) {
                    ImGui.PushID($"emote_{emote.Id}");

                    if (!emote.IsUnlocked) {
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 0.5f));
                    }

                    if (ImGui.ImageButton(iconWrap.Handle, new Vector2(IconSize, IconSize))) {
                        if (emote.IsUnlocked) {
                            this.executionService.ExecuteEmote(emote.Id);
                        }
                    }

                    if (!emote.IsUnlocked) {
                        ImGui.PopStyleColor();
                    }

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(emote.IsModded ? $"★ {emote.Name}\n{emote.LocalizedCommand}" : $"{emote.Name}\n{emote.LocalizedCommand}");
                    }

                    ImGui.PopID();
                    drawn = true;
                }
            }
            catch (IconNotFoundException) { }
        }

        if (!drawn) {
            ImGui.Button("?", new Vector2(IconSize, IconSize));
        }
    }

    private void DrawEmptySlot() {
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0.2f));
        ImGui.Button("##empty", new Vector2(IconSize, IconSize));
        ImGui.PopStyleColor();
    }

    private void DrawPagination(int totalPages) {
        ImGui.PushFont(UiBuilder.IconFont);

        if (ImGui.Button(FontAwesomeIcon.ChevronLeft.ToIconString()) && this.currentPage > 0) {
            this.currentPage--;
        }

        ImGui.PopFont();
        ImGui.SameLine();

        ImGui.Text($"{this.currentPage + 1}/{totalPages}");

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);

        if (ImGui.Button(FontAwesomeIcon.ChevronRight.ToIconString()) && this.currentPage < totalPages - 1) {
            this.currentPage++;
        }

        ImGui.PopFont();
    }
}