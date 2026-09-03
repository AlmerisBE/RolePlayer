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
        this.IsOpen = true;
    }

    public override void PreDraw() {
        // Application du verrouillage de la fenêtre si demandé
        if (this.config.IsLocked) {
            this.Flags |= ImGuiWindowFlags.NoMove;
        }
        else {
            this.Flags &= ~ImGuiWindowFlags.NoMove;
        }

        // Réduction drastique des espacements pour un effet "Barre de raccourcis native"
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4f, 4f));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2f, 2f));
    }

    public override void Draw() {
        if (!this.config.IsVisible) {
            return;
        }

        var allEmotes = this.emoteCacheProvider();
        var resolvedEmotes = this.resolverService.ResolveEmotesForHotbar(this.config, allEmotes);

        if (resolvedEmotes.Count == 0) {
            return;
        }

        int totalPages = (int)Math.Ceiling(resolvedEmotes.Count / (double)MaxItemsPerPage);
        if (this.currentPage >= totalPages && totalPages > 0) {
            this.currentPage = totalPages - 1;
        }

        if (totalPages == 0) {
            this.currentPage = 0;
        }

        var displayedEmotes = resolvedEmotes.Skip(this.currentPage * MaxItemsPerPage).Take(MaxItemsPerPage).ToList();

        // Taille dynamique : Si nous avons moins d'emotes que de colonnes prévues, nous réduisons le tableau
        int maxColumns = this.GetColumnsForLayout(this.config.Layout);
        int actualColumns = Math.Min(maxColumns, displayedEmotes.Count);
        if (actualColumns <= 0) {
            actualColumns = 1;
        }

        if (ImGui.BeginTable($"HotbarGrid_{this.config.Id}", actualColumns, ImGuiTableFlags.SizingFixedFit)) {
            for (int i = 0; i < displayedEmotes.Count; i++) {
                if (i % actualColumns == 0) {
                    ImGui.TableNextRow();
                }

                ImGui.TableNextColumn();
                this.DrawEmoteIcon(displayedEmotes[i]);
            }
            ImGui.EndTable();
        }

        if (totalPages > 1) {
            this.DrawPagination(totalPages);
        }
    }

    public override void PostDraw() {
        ImGui.PopStyleVar(2);
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
        try {
            var lookup = new GameIconLookup { IconId = emote.IconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null) {
                ImGui.PushID($"emote_{emote.Id}");

                if (ImGui.ImageButton(iconWrap.Handle, new Vector2(IconSize, IconSize))) {
                    this.executionService.ExecuteEmote(emote.Id);
                }

                if (ImGui.IsItemHovered()) {
                    string tooltipText = emote.IsModded ? $"★ {emote.Name}\nMod: {emote.ModName}\n{emote.LocalizedCommand}" : $"{emote.Name}\n{emote.LocalizedCommand}";
                    ImGui.SetTooltip(tooltipText);
                }

                ImGui.PopID();
            }
        }
        catch (IconNotFoundException) { }
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