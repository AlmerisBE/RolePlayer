namespace RolePlayer.UI.Hotbar.Services;

using RolePlayer.Core.Configuration.Models;
using RolePlayer.UI.Hotbar.Contracts;
using System;
using System.Numerics;

public class HotbarLayoutService : IHotbarLayoutService {
    private const float BaseIconSize = 43f;
    private const float WindowPaddingX = 4f;
    private const float WindowPaddingY = 4f;
    private const float CellPadding = 1f;
    private const float ItemSpacing = 1f;

    public float GetCurrentIconSize(float scale) => BaseIconSize * scale;
    public float GetCurrentPaddingX(float scale) => WindowPaddingX * scale;
    public float GetCurrentPaddingY(float scale) => WindowPaddingY * scale;
    public float GetCurrentCellPadding(float scale) => CellPadding * scale;
    public float GetItemSpacing() => ItemSpacing;

    public int GetColumnsForLayout(HotbarLayout layout) {
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

    public int GetLogicalIndex(int row, int col, int maxRows, int maxColumns, HotbarFillDirection direction) {
        return direction switch {
            HotbarFillDirection.LeftToRight => row * maxColumns + col,
            HotbarFillDirection.RightToLeft => row * maxColumns + (maxColumns - 1 - col),
            HotbarFillDirection.TopToBottom => col * maxRows + row,
            HotbarFillDirection.BottomToTop => col * maxRows + (maxRows - 1 - row),
            _ => row * maxColumns + col
        };
    }

    public Vector2 GetGridSize(HotbarConfig config, int itemCount) {
        float currentIconSize = this.GetCurrentIconSize(config.Scale);
        float currentPaddingX = this.GetCurrentPaddingX(config.Scale);
        float currentPaddingY = this.GetCurrentPaddingY(config.Scale);
        float currentCellPadding = this.GetCurrentCellPadding(config.Scale);

        if (itemCount == 0) return new Vector2(currentIconSize + (currentPaddingX * 2), currentIconSize + (currentPaddingY * 2));

        int maxColumns = this.GetColumnsForLayout(config.Layout);
        int maxRows = (int)Math.Ceiling(config.ButtonCount / (double)maxColumns);

        float width = (currentPaddingX * 2) + (maxColumns * (currentIconSize + (currentCellPadding * 2)));
        float height = (currentPaddingY * 2) + (maxRows * (currentIconSize + (currentCellPadding * 2)));

        return new Vector2(width, height);
    }

    public Vector2 GetPivot(HotbarAnchor anchor) {
        return anchor switch {
            HotbarAnchor.TopLeft => new Vector2(0, 0),
            HotbarAnchor.TopRight => new Vector2(1, 0),
            HotbarAnchor.BottomLeft => new Vector2(0, 1),
            HotbarAnchor.BottomRight => new Vector2(1, 1),
            _ => new Vector2(0, 0)
        };
    }

    public Vector2 CalculateAnchorPosition(Vector2 pos, Vector2 size, HotbarAnchor anchor) {
        return anchor switch {
            HotbarAnchor.TopLeft => pos,
            HotbarAnchor.TopRight => new Vector2(pos.X + size.X, pos.Y),
            HotbarAnchor.BottomLeft => new Vector2(pos.X, pos.Y + size.Y),
            HotbarAnchor.BottomRight => new Vector2(pos.X + size.X, pos.Y + size.Y),
            _ => pos
        };
    }
}