namespace RolePlayer.UI.Hotbar.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Contracts;
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
    private IEmoteExecutionService emoteExecutionService;
    private IMacroExecutionService macroExecutionService;
    private ITextureProvider textureProvider;
    private IEmoteCache emoteCache;
    private IConfigurationService configService;
    private ICondition condition;
    private ILocalizationService localization;

    private List<ResolvedHotbarItem> cachedItems = new();
    private int currentPage = 0;
    private bool wasOpen = false;

    // Dimensions Constants
    private const float BaseIconSize = 43f;
    private const float WindowPaddingX = 4f;
    private const float WindowPaddingY = 4f;
    private const float CellPadding = 1f;
    private const float ItemSpacing = 1f;

    private float CurrentIconSize => BaseIconSize * this.config.Scale;
    private float CurrentWindowPaddingX => WindowPaddingX * this.config.Scale;
    private float CurrentWindowPaddingY => WindowPaddingY * this.config.Scale;
    private float CurrentCellPadding => CellPadding * this.config.Scale;

    public HotbarWindow(
        HotbarConfig config,
        IHotbarResolverService resolverService,
        IEmoteExecutionService emoteExecutionService,
        IMacroExecutionService macroExecutionService,
        ITextureProvider textureProvider,
        IEmoteCache emoteCache,
        IConfigurationService configService,
        ICondition condition,
        ILocalizationService localization)
        : base($"RolePlayer_Hotbar_{config.Id}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize) {

        this.config = config;
        this.resolverService = resolverService;
        this.emoteExecutionService = emoteExecutionService;
        this.macroExecutionService = macroExecutionService;
        this.textureProvider = textureProvider;
        this.emoteCache = emoteCache;
        this.configService = configService;
        this.condition = condition;
        this.localization = localization;

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
        float minWidth = this.CurrentIconSize + (this.CurrentWindowPaddingX * 2);
        float minHeight = this.CurrentIconSize + (this.CurrentWindowPaddingY * 2);

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(minWidth, minHeight),
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

    private bool EvaluateHotbarVisibility() {
        if (!this.configService.GetConfig().EnableHotbars) return true;
        if (this.condition[ConditionFlag.WatchingCutscene]) return true;
        if (this.condition[ConditionFlag.BetweenAreas] || this.condition[ConditionFlag.BetweenAreas51]) return true;
        if (this.condition[ConditionFlag.LoggingOut]) return true;
        if (this.config.HideInCombat && this.condition[ConditionFlag.InCombat]) return true;
        if (this.config.HideInDuty && (this.condition[ConditionFlag.BoundByDuty] || this.condition[ConditionFlag.BoundByDuty56])) return true;

        return false;
    }

    public override void Update() {
        try {
            bool hide = this.EvaluateHotbarVisibility();
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
                    var gridSize = this.GetGridSize();
                    var pivot = this.GetPivot(this.config.Anchor);

                    var topLeft = this.config.AnchorPosition - new Vector2(gridSize.X * pivot.X, gridSize.Y * pivot.Y);
                    ImGui.SetNextWindowPos(topLeft, ImGuiCond.Always, new Vector2(0, 0));
                }
            }
        }
        else {
            this.Flags &= ~ImGuiWindowFlags.NoMove;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(WindowPaddingX, WindowPaddingY));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(ItemSpacing, ItemSpacing));
    }

    public override void Draw() {
        ImGui.SetWindowFontScale(this.config.Scale);

        int maxItemsPerPage = this.config.ButtonCount;
        int totalPages = (int)Math.Ceiling(this.cachedItems.Count / (double)maxItemsPerPage);

        if (this.cachedItems.Count > 0) {
            var displayedItems = this.cachedItems.Skip(this.currentPage * maxItemsPerPage).Take(maxItemsPerPage).ToList();

            int maxColumns = this.GetColumnsForLayout(this.config.Layout);
            int maxRows = (int)Math.Ceiling(this.config.ButtonCount / (double)maxColumns);

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(CurrentCellPadding, CurrentCellPadding));
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);

            if (ImGui.BeginTable($"HotbarGrid_{this.config.Id}", maxColumns, ImGuiTableFlags.SizingFixedFit)) {
                for (int col = 0; col < maxColumns; col++) ImGui.TableSetupColumn($"col_{col}", ImGuiTableColumnFlags.WidthFixed, this.CurrentIconSize);

                for (int row = 0; row < maxRows; row++) {
                    ImGui.TableNextRow();
                    for (int col = 0; col < maxColumns; col++) {
                        ImGui.TableNextColumn();

                        int logicalIndex = this.GetLogicalIndex(row, col, maxRows, maxColumns, this.config.FillDirection);

                        if (logicalIndex < displayedItems.Count) this.DrawHotbarItemIcon(displayedItems[logicalIndex]);
                        else ImGui.Dummy(new Vector2(this.CurrentIconSize, this.CurrentIconSize));
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
            ImGui.Dummy(new Vector2(this.CurrentIconSize, this.CurrentIconSize));
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_common_empty"));
        }

        if (!this.config.IsLocked || !this.config.PositionInitialized) {
            var pos = ImGui.GetWindowPos();
            var gridSize = this.GetGridSize();
            this.config.AnchorPosition = this.CalculateAnchorPosition(pos, gridSize, this.config.Anchor);
            this.config.PositionInitialized = true;
        }

        ImGui.SetWindowFontScale(1.0f);
    }

    public override void PostDraw() {
        ImGui.PopStyleVar(2);
    }

    private int GetLogicalIndex(int row, int col, int maxRows, int maxColumns, HotbarFillDirection direction) {
        return direction switch {
            HotbarFillDirection.LeftToRight => row * maxColumns + col,
            HotbarFillDirection.RightToLeft => row * maxColumns + (maxColumns - 1 - col),
            HotbarFillDirection.TopToBottom => col * maxRows + row,
            HotbarFillDirection.BottomToTop => col * maxRows + (maxRows - 1 - row),
            _ => row * maxColumns + col
        };
    }

    private Vector2 GetGridSize() {
        if (this.cachedItems.Count == 0) return new Vector2(this.CurrentIconSize + (this.CurrentWindowPaddingX * 2), this.CurrentIconSize + (this.CurrentWindowPaddingY * 2));

        int maxColumns = this.GetColumnsForLayout(this.config.Layout);
        int maxRows = (int)Math.Ceiling(this.config.ButtonCount / (double)maxColumns);

        float width = (this.CurrentWindowPaddingX * 2) + (maxColumns * (this.CurrentIconSize + (this.CurrentCellPadding * 2)));
        float height = (this.CurrentWindowPaddingY * 2) + (maxRows * (this.CurrentIconSize + (this.CurrentCellPadding * 2)));

        return new Vector2(width, height);
    }

    private Vector2 GetPivot(HotbarAnchor anchor) {
        return anchor switch {
            HotbarAnchor.TopLeft => new Vector2(0, 0),
            HotbarAnchor.TopRight => new Vector2(1, 0),
            HotbarAnchor.BottomLeft => new Vector2(0, 1),
            HotbarAnchor.BottomRight => new Vector2(1, 1),
            _ => new Vector2(0, 0)
        };
    }

    private Vector2 CalculateAnchorPosition(Vector2 pos, Vector2 size, HotbarAnchor anchor) {
        return anchor switch {
            HotbarAnchor.TopLeft => pos,
            HotbarAnchor.TopRight => new Vector2(pos.X + size.X, pos.Y),
            HotbarAnchor.BottomLeft => new Vector2(pos.X, pos.Y + size.Y),
            HotbarAnchor.BottomRight => new Vector2(pos.X + size.X, pos.Y + size.Y),
            _ => pos
        };
    }

    private int GetColumnsForLayout(HotbarLayout layout) {
        return layout switch {
            HotbarLayout.Grid12x1 => 12,
            HotbarLayout.Grid6x2 => 6,
            HotbarLayout.Grid4x3 => 4,
            HotbarLayout.Grid3x4 => 3,
            HotbarLayout.Grid2x6 => 2,
            HotbarLayout.Grid1x12 => 1,

            HotbarLayout.Grid16x1 => 16,
            HotbarLayout.Grid8x2 => 8,
            HotbarLayout.Grid4x4 => 4,
            HotbarLayout.Grid2x8 => 2,
            HotbarLayout.Grid1x16 => 1,

            HotbarLayout.Grid18x2 => 18,
            HotbarLayout.Grid12x3 => 12,
            HotbarLayout.Grid9x4 => 9,
            HotbarLayout.Grid6x6 => 6,
            HotbarLayout.Grid4x9 => 4,
            HotbarLayout.Grid3x12 => 3,
            HotbarLayout.Grid2x18 => 2,

            _ => 12
        };
    }

    private void DrawHotbarItemIcon(ResolvedHotbarItem item) {
        bool idPushed = false;
        try {
            var iconLookup = new GameIconLookup { IconId = item.IconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

            if (iconWrap == null) {
                ImGui.Dummy(new Vector2(this.CurrentIconSize, this.CurrentIconSize));
                return;
            }

            ImGui.PushID($"item_{(item.EmoteId.HasValue ? item.EmoteId.ToString() : item.MacroId.ToString())}");
            idPushed = true;

            var size = new Vector2(this.CurrentIconSize, this.CurrentIconSize);
            var cursorPos = ImGui.GetCursorScreenPos();

            bool isClicked = ImGui.InvisibleButton("btn", size);
            bool isHovered = ImGui.IsItemHovered();
            bool isActive = ImGui.IsItemActive();

            var drawList = ImGui.GetWindowDrawList();

            Vector2 drawPos = cursorPos;
            if (isActive) {
                drawPos.X += 1f;
                drawPos.Y += 1f;
            }

            drawList.AddRectFilled(drawPos, drawPos + size, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f)), 4f);
            drawList.AddImage(iconWrap.Handle, drawPos, drawPos + size);

            drawList.AddRect(drawPos, drawPos + size, ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 1f)), 4f, ImDrawFlags.None, 2f);
            drawList.AddRect(drawPos + new Vector2(1f, 1f), drawPos + size - new Vector2(1f, 1f), ImGui.GetColorU32(new Vector4(0.6f, 0.6f, 0.6f, 1f)), 3f, ImDrawFlags.None, 1f);

            if (isHovered && !isActive) drawList.AddRectFilled(drawPos, drawPos + size, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.2f)), 4f);
            if (isActive) drawList.AddRectFilled(drawPos, drawPos + size, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.3f)), 4f);

            if (isClicked) {
                if (item.EmoteId.HasValue) this.emoteExecutionService.ExecuteEmote(item.EmoteId.Value);
                if (item.MacroId.HasValue && item.MacroReference != null) this.macroExecutionService.Execute(item.MacroReference);
            }

            if (item.HasVariations) {
                ImGui.PushFont(UiBuilder.IconFont);
                var indicatorText = FontAwesomeIcon.Sync.ToIconString();
                var textSize = ImGui.CalcTextSize(indicatorText);
                ImGui.PopFont();

                var indicatorPos = new Vector2(drawPos.X + this.CurrentIconSize - textSize.X - 2f, drawPos.Y + this.CurrentIconSize - textSize.Y - 2f);

                drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), new Vector2(indicatorPos.X + 1, indicatorPos.Y + 1), ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f)), indicatorText);
                drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), indicatorPos, ImGui.GetColorU32(new Vector4(0.25f, 0.86f, 0.25f, 1f)), indicatorText);
            }

            if (isHovered) {
                string tooltipText = item.IsModded ? $"★ {item.Name}\n{this.localization.Translate("hotbar_tooltip_mod")} {item.ModName}\n{item.CommandText}" : $"{item.Name}\n{item.CommandText}";
                if (item.HasVariations) tooltipText += $"\n{this.localization.Translate("hotbar_tooltip_variation")}";

                ImGui.SetTooltip(tooltipText);
            }
        }
        catch (Exception) {
            ImGui.Dummy(new Vector2(this.CurrentIconSize, this.CurrentIconSize));
        }
        finally {
            if (idPushed) ImGui.PopID();
        }
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