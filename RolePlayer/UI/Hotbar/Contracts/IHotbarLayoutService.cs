namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.Core.Configuration.Models;
using System.Numerics;

public interface IHotbarLayoutService {
    float GetCurrentIconSize(float scale);
    float GetCurrentPaddingX(float scale);
    float GetCurrentPaddingY(float scale);
    float GetCurrentCellPadding(float scale);
    float GetItemSpacing();

    int GetColumnsForLayout(HotbarLayout layout);
    int GetLogicalIndex(int row, int col, int maxRows, int maxColumns, HotbarFillDirection direction);
    Vector2 GetGridSize(HotbarConfig config, int itemCount);
    Vector2 CalculateAnchorPosition(Vector2 pos, Vector2 gridSize, HotbarAnchor anchor);
    Vector2 GetPivot(HotbarAnchor anchor);
}