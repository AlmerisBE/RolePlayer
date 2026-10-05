namespace RolePlayer.UI.MainWindow.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Localization.Contracts;
using System.Collections.Generic;
using System.Numerics;

public class HotbarPreviewComponent {
    private IConfigurationService configService;
    private HotbarManagerComponent hotbarManager;
    private IHotbarResolverService resolverService;
    private ITextureProvider textureProvider;
    private ILocalizationService localization;

    private int draggedItemIndex = -1;
    private const float IconSize = 24f;
    private const string PayloadId = "RP_HOTBAR_DRAG";

    private byte[] dummyPayload = new byte[1] { 1 };

    public HotbarPreviewComponent(
        IConfigurationService configService,
        HotbarManagerComponent hotbarManager,
        IHotbarResolverService resolverService,
        ITextureProvider textureProvider,
        ILocalizationService localization) {

        this.configService = configService;
        this.hotbarManager = hotbarManager;
        this.resolverService = resolverService;
        this.textureProvider = textureProvider;
        this.localization = localization;
    }

    public void Draw(HotbarConfig config, IReadOnlyList<ResolvedHotbarItem> resolvedItems) {
        if (resolvedItems.Count == 0) {
            ImGui.TextDisabled(this.localization.Translate("config_common_empty"));
            return;
        }

        float availWidth = ImGui.GetContentRegionAvail().X;
        int cols = (int)(availWidth / (IconSize + 8f));
        if (cols < 1) cols = 1;

        if (ImGui.BeginTable($"PreviewGrid_{config.Id}", cols, ImGuiTableFlags.SizingFixedFit)) {
            for (int i = 0; i < cols; i++) ImGui.TableSetupColumn($"col_{i}", ImGuiTableColumnFlags.WidthFixed, IconSize);

            for (int i = 0; i < resolvedItems.Count; i++) {
                if (i % cols == 0) ImGui.TableNextRow();
                ImGui.TableNextColumn();
                this.DrawDraggableIcon(config, resolvedItems, i);
            }
            ImGui.EndTable();
        }
    }

    private void DrawDraggableIcon(HotbarConfig config, IReadOnlyList<ResolvedHotbarItem> items, int index) {
        var item = items[index];
        ImGui.PushID($"preview_item_{index}");

        try {
            var lookup = new GameIconLookup { IconId = item.IconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);

            if (iconWrap != null) ImGui.ImageButton(iconWrap.Handle, new Vector2(IconSize, IconSize));
            else ImGui.Button($"##fallback_{index}", new Vector2(IconSize, IconSize));

            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) {
                this.draggedItemIndex = index;

                ImGui.SetDragDropPayload(PayloadId, this.dummyPayload);

                ImGui.Text(item.Name);
                if (iconWrap != null) ImGui.Image(iconWrap.Handle, new Vector2(IconSize, IconSize));

                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget()) {
                ImGui.AcceptDragDropPayload(PayloadId);

                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && this.draggedItemIndex != -1) {
                    this.resolverService.UpdateCustomOrder(config, items, this.draggedItemIndex, index);

                    this.configService.Save();
                    this.hotbarManager.RequestRefresh();

                    this.draggedItemIndex = -1;
                }
                ImGui.EndDragDropTarget();
            }
        }
        catch (IconNotFoundException) {
            ImGui.Button($"##broken_{index}", new Vector2(IconSize, IconSize));
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(item.Name);

        ImGui.PopID();
    }
}