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
using System;
using System.Collections.Generic;
using System.Numerics;

public class HotbarPreviewComponent {
    private IConfigurationService configService;
    private HotbarManagerComponent hotbarManager;
    private IHotbarResolverService resolverService;
    private ITextureProvider textureProvider;
    private ILocalizationService localization;

    private int draggedItemIndex = -1;
    private const float IconSize = 41f;

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

            if (iconWrap != null) ImGui.Image(iconWrap.Handle, new Vector2(IconSize, IconSize));
            else ImGui.Dummy(new Vector2(IconSize, IconSize));

            string payloadId = $"HOTBAR_DRAG_{config.Id}";

            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) {
                this.draggedItemIndex = index;
                ImGui.SetDragDropPayload(payloadId, ReadOnlySpan<byte>.Empty);

                ImGui.Text(item.Name);
                if (iconWrap != null) ImGui.Image(iconWrap.Handle, new Vector2(IconSize / 2, IconSize / 2));

                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget()) {
                ImGui.AcceptDragDropPayload(payloadId);

                // Evaluates to true only on the exact frame the user releases the drag over this target
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
            ImGui.Dummy(new Vector2(IconSize, IconSize));
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(item.Name);

        ImGui.PopID();
    }
}