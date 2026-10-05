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

public class HotbarWindow : Window {
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

    private const float BaseIconSize = 41f;
    private float CurrentIconSize => BaseIconSize * this.config.Scale;

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

        this.RefreshContent();
    }

    public void UpdateConfig(HotbarConfig newConfig) {
        this.config = newConfig;
        this.UpdateSizeConstraints();
        this.RefreshContent();
    }

    private void UpdateSizeConstraints() {
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(this.CurrentIconSize + 16f, this.CurrentIconSize + 16f),
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
        if (this.config.HideInCombat && this.condition[ConditionFlag.InCombat]) return true;
        if (this.config.HideInDuty && (this.condition[ConditionFlag.BoundByDuty] || this.condition[ConditionFlag.BoundByDuty56])) return true;

        return false;
    }

    public override void Update() {
        try {
            bool hide = this.EvaluateHotbarVisibility();
            this.IsOpen = this.config.IsVisible && !hide;
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
                var gridSize = this.GetGridSize();
                var pivot = this.GetPivot(this.config.Anchor);

                // On calcule manuellement l'origine TopLeft pour s'isoler de la taille globale de la fenêtre
                var topLeft = this.config.AnchorPosition - new Vector2(gridSize.X * pivot.X, gridSize.Y * pivot.Y);

                // Le pivot est forcé à (0,0) pour que la pagination s'étende toujours vers le bas sans décaler la grille
                ImGui.SetNextWindowPos(topLeft, ImGuiCond.Always, new Vector2(0, 0));
            }
        }
        else {
            this.Flags &= ~ImGuiWindowFlags.NoMove;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8f, 8f));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2f, 2f));
    }

    public override void Draw() {
        ImGui.SetWindowFontScale(this.config.Scale);

        int maxItemsPerPage = this.config.ButtonCount;
        int totalPages = (int)Math.Ceiling(this.cachedItems.Count / (double)maxItemsPerPage);

        if (this.cachedItems.Count > 0) {
            var displayedItems = this.cachedItems.Skip(this.currentPage * maxItemsPerPage).Take(maxItemsPerPage).ToList();

            int maxColumns = this.GetColumnsForLayout(this.config.Layout);
            int actualColumns = Math.Max(1, Math.Min(maxColumns, displayedItems.Count));

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(2f, 2f));
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);

            if (ImGui.BeginTable($"HotbarGrid_{this.config.Id}", actualColumns, ImGuiTableFlags.SizingFixedFit)) {
                for (int col = 0; col < actualColumns; col++) ImGui.TableSetupColumn($"col_{col}", ImGuiTableColumnFlags.WidthFixed, this.CurrentIconSize);

                for (int i = 0; i < displayedItems.Count; i++) {
                    if (i % actualColumns == 0) ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    this.DrawHotbarItemIcon(displayedItems[i]);
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

    private Vector2 GetGridSize() {
        if (this.cachedItems.Count == 0) return new Vector2(this.CurrentIconSize + 16f, this.CurrentIconSize + 16f);

        int maxItemsPerPage = this.config.ButtonCount;
        var displayedItems = this.cachedItems.Skip(this.currentPage * maxItemsPerPage).Take(maxItemsPerPage).ToList();

        if (displayedItems.Count == 0) return new Vector2(this.CurrentIconSize + 16f, this.CurrentIconSize + 16f);

        int maxColumns = this.GetColumnsForLayout(this.config.Layout);
        int actualColumns = Math.Max(1, Math.Min(maxColumns, displayedItems.Count));
        int actualRows = (int)Math.Ceiling(displayedItems.Count / (double)actualColumns);

        // 16f = WindowPadding.X/Y * 2, 4f = CellPadding.X/Y * 2
        float width = 16f + (actualColumns * (this.CurrentIconSize + 4f));
        float height = 16f + (actualRows * (this.CurrentIconSize + 4f));

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
            _ => 16
        };
    }

    private void DrawHotbarItemIcon(ResolvedHotbarItem item) {
        try {
            var lookup = new GameIconLookup { IconId = item.IconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null) {
                ImGui.PushID($"item_{(item.EmoteId.HasValue ? item.EmoteId.ToString() : item.MacroId.ToString())}");
                var cursorPos = ImGui.GetCursorScreenPos();

                if (ImGui.ImageButton(iconWrap.Handle, new Vector2(this.CurrentIconSize, this.CurrentIconSize))) {
                    if (item.EmoteId.HasValue) this.emoteExecutionService.ExecuteEmote(item.EmoteId.Value);
                    else if (item.MacroId.HasValue && item.MacroReference != null) this.macroExecutionService.Execute(item.MacroReference);
                }

                if (item.HasVariations) {
                    var drawList = ImGui.GetWindowDrawList();
                    ImGui.PushFont(UiBuilder.IconFont);
                    var indicatorText = FontAwesomeIcon.Sync.ToIconString();
                    var textSize = ImGui.CalcTextSize(indicatorText);
                    ImGui.PopFont();

                    var indicatorPos = new Vector2(cursorPos.X + this.CurrentIconSize - textSize.X - 2f, cursorPos.Y + this.CurrentIconSize - textSize.Y - 2f);

                    drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), new Vector2(indicatorPos.X + 1, indicatorPos.Y + 1), 0xFF000000, indicatorText);
                    drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), indicatorPos, 0xFF40DD40, indicatorText);
                }

                if (ImGui.IsItemHovered()) {
                    string tooltipText = item.IsModded ? $"★ {item.Name}\n{this.localization.Translate("hotbar_tooltip_mod")} {item.ModName}\n{item.CommandText}" : $"{item.Name}\n{item.CommandText}";
                    if (item.HasVariations) tooltipText += $"\n{this.localization.Translate("hotbar_tooltip_variation")}";

                    ImGui.SetTooltip(tooltipText);
                }

                ImGui.PopID();
            }
            else {
                ImGui.Dummy(new Vector2(this.CurrentIconSize, this.CurrentIconSize));
            }
        }
        catch (Exception) {
            ImGui.Dummy(new Vector2(this.CurrentIconSize, this.CurrentIconSize));
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