namespace RolePlayer.UI.Hotbar.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class HotbarWindow : Window, IDisposable {
    private HotbarConfig config;
    private IHotbarResolverService resolverService;
    private IEmoteCache emoteCache;
    private ILocalizationService localization;
    private IHotbarVisibilityService visibilityService;
    private IHotbarLayoutService layoutService;
    private IHotbarButtonRenderer buttonRenderer;

    private List<ResolvedHotbarItem> cachedItems = new();
    private int currentPage = 0;
    private bool wasOpen = false;

    public HotbarWindow(
        HotbarConfig config,
        IHotbarResolverService resolverService,
        IEmoteCache emoteCache,
        ILocalizationService localization,
        IHotbarVisibilityService visibilityService,
        IHotbarLayoutService layoutService,
        IHotbarButtonRenderer buttonRenderer)
        : base($"RolePlayer_Hotbar_{config.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize) {

        this.config = config;
        this.resolverService = resolverService;
        this.emoteCache = emoteCache;
        this.localization = localization;
        this.visibilityService = visibilityService;
        this.layoutService = layoutService;
        this.buttonRenderer = buttonRenderer;

        this.UpdateSizeConstraints();
        this.SizeCondition = ImGuiCond.Always;
        this.IsOpen = config.IsVisible;

        this.emoteCache.CacheUpdated += this.RefreshContent;
        this.RefreshContent();
    }

    public void Dispose() {
        this.emoteCache.CacheUpdated -= this.RefreshContent;
        GC.SuppressFinalize(this);
    }

    public void UpdateConfig(HotbarConfig newConfig) {
        this.config = newConfig;
        this.UpdateSizeConstraints();
        this.RefreshContent();
    }

    private void UpdateSizeConstraints() {
        float iconSize = this.layoutService.GetCurrentIconSize(this.config.Scale);
        float paddingX = this.layoutService.GetCurrentPaddingX(this.config.Scale);
        float paddingY = this.layoutService.GetCurrentPaddingY(this.config.Scale);

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(iconSize + (paddingX * 2), iconSize + (paddingY * 2)),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void RefreshContent() {
        var allEmotes = this.emoteCache.GetCachedEmotes();
        this.cachedItems = this.resolverService.ResolveItemsForHotbar(this.config, allEmotes).ToList();

        int maxItemsPerPage = this.config.ButtonCount;
        int totalPages = (int)Math.Ceiling(this.cachedItems.Count / (double)maxItemsPerPage);

        if (this.currentPage >= totalPages && totalPages > 0) this.currentPage = totalPages - 1;
        if (totalPages == 0) this.currentPage = 0;
    }

    public override void Update() {
        try {
            bool hide = this.visibilityService.ShouldHide(this.config);
            this.IsOpen = this.config.IsVisible && !hide;

            if (this.IsOpen && !this.wasOpen) this.RefreshContent();

            this.wasOpen = this.IsOpen;
        }
        catch {
            this.IsOpen = this.config.IsVisible;
        }

        this.BgAlpha = this.config.IsLocked ? 0.0f : 0.7f;
    }

    public override void PreDraw() {
        if (this.config.IsLocked) {
            this.Flags |= ImGuiWindowFlags.NoMove;
            if (this.config.PositionInitialized) {
                if (float.IsNaN(this.config.AnchorPosition.X) || float.IsNaN(this.config.AnchorPosition.Y) ||
                    float.IsInfinity(this.config.AnchorPosition.X) || float.IsInfinity(this.config.AnchorPosition.Y)) {
                    this.config.PositionInitialized = false;
                }
                else {
                    var gridSize = this.layoutService.GetGridSize(this.config, this.cachedItems.Count);
                    var pivot = this.layoutService.GetPivot(this.config.Anchor);

                    var topLeft = this.config.AnchorPosition - new Vector2(gridSize.X * pivot.X, gridSize.Y * pivot.Y);
                    ImGui.SetNextWindowPos(topLeft, ImGuiCond.Always, new Vector2(0, 0));
                }
            }
        }
        else {
            this.Flags &= ~ImGuiWindowFlags.NoMove;
        }

        float paddingX = this.layoutService.GetCurrentPaddingX(this.config.Scale);
        float paddingY = this.layoutService.GetCurrentPaddingY(this.config.Scale);
        float itemSpacing = this.layoutService.GetItemSpacing();

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(paddingX, paddingY));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(itemSpacing, itemSpacing));
    }

    public override void Draw() {
        ImGui.SetWindowFontScale(this.config.Scale);

        int maxItemsPerPage = this.config.ButtonCount;
        int totalPages = (int)Math.Ceiling(this.cachedItems.Count / (double)maxItemsPerPage);
        float iconSize = this.layoutService.GetCurrentIconSize(this.config.Scale);

        if (this.cachedItems.Count > 0) {
            var displayedItems = this.cachedItems.Skip(this.currentPage * maxItemsPerPage).Take(maxItemsPerPage).ToList();

            int maxColumns = this.layoutService.GetColumnsForLayout(this.config.Layout);
            int maxRows = (int)Math.Ceiling(this.config.ButtonCount / (double)maxColumns);
            float cellPadding = this.layoutService.GetCurrentCellPadding(this.config.Scale);

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(cellPadding, cellPadding));
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);

            if (ImGui.BeginTable($"HotbarGrid_{this.config.Id}", maxColumns, ImGuiTableFlags.SizingFixedFit)) {
                for (int col = 0; col < maxColumns; col++) ImGui.TableSetupColumn($"col_{col}", ImGuiTableColumnFlags.WidthFixed, iconSize);

                for (int row = 0; row < maxRows; row++) {
                    ImGui.TableNextRow();
                    for (int col = 0; col < maxColumns; col++) {
                        ImGui.TableNextColumn();

                        int logicalIndex = this.layoutService.GetLogicalIndex(row, col, maxRows, maxColumns, this.config.FillDirection);

                        if (logicalIndex < displayedItems.Count) this.buttonRenderer.Draw(displayedItems[logicalIndex], iconSize);
                        else ImGui.Dummy(new Vector2(iconSize, iconSize));
                    }
                }
                ImGui.EndTable();
            }

            ImGui.PopStyleColor();
            ImGui.PopStyleVar(2);

            if (totalPages > 1) {
                ImGui.Spacing();
                this.DrawPagination(totalPages);
            }
        }
        else if (!this.config.IsLocked) {
            ImGui.Dummy(new Vector2(iconSize, iconSize));
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_common_empty"));
        }

        if (!this.config.IsLocked || !this.config.PositionInitialized) {
            var pos = ImGui.GetWindowPos();
            var gridSize = this.layoutService.GetGridSize(this.config, this.cachedItems.Count);
            this.config.AnchorPosition = this.layoutService.CalculateAnchorPosition(pos, gridSize, this.config.Anchor);
            this.config.PositionInitialized = true;
        }

        ImGui.SetWindowFontScale(1.0f);
    }

    public override void PostDraw() {
        ImGui.PopStyleVar(2);
    }

    private void DrawPagination(int totalPages) {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.ChevronLeft.ToIconString()) && this.currentPage > 0) this.currentPage--;
        ImGui.PopFont();

        ImGui.SameLine();
        ImGui.Text($"{this.currentPage + 1}/{totalPages}");
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.ChevronRight.ToIconString()) && this.currentPage < totalPages - 1) this.currentPage++;
        ImGui.PopFont();
    }
}