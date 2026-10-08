namespace RolePlayer.UI.Hotbar.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Input.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Numerics;

public class HotbarButtonRenderer : IHotbarButtonRenderer {
    private ITextureProvider textureProvider;
    private IEmoteExecutionService emoteExecutionService;
    private IMacroExecutionService macroExecutionService;
    private IHotkeyService hotkeyService;
    private ILocalizationService localization;

    public HotbarButtonRenderer(
        ITextureProvider textureProvider,
        IEmoteExecutionService emoteExecutionService,
        IMacroExecutionService macroExecutionService,
        IHotkeyService hotkeyService,
        ILocalizationService localization) {

        this.textureProvider = textureProvider;
        this.emoteExecutionService = emoteExecutionService;
        this.macroExecutionService = macroExecutionService;
        this.hotkeyService = hotkeyService;
        this.localization = localization;
    }

    public void Draw(ResolvedHotbarItem item, float iconSize) {
        bool idPushed = false;
        try {
            var iconLookup = new GameIconLookup { IconId = item.IconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

            if (iconWrap == null) {
                ImGui.Dummy(new Vector2(iconSize, iconSize));
                return;
            }

            ImGui.PushID($"item_{(item.EmoteId.HasValue ? item.EmoteId.ToString() : item.MacroId.ToString())}");
            idPushed = true;

            var size = new Vector2(iconSize, iconSize);
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

            this.DrawVariationIndicator(item, drawPos, iconSize, drawList);
            this.DrawHotkeyOverlay(item, drawPos, drawList);

            if (isHovered) {
                string tooltipText = item.IsModded ? $"★ {item.Name}\n{this.localization.Translate("hotbar_tooltip_mod")} {item.ModName}\n{item.CommandText}" : $"{item.Name}\n{item.CommandText}";
                if (item.HasVariations) tooltipText += $"\n{this.localization.Translate("hotbar_tooltip_variation")}";
                ImGui.SetTooltip(tooltipText);
            }
        }
        catch (Exception) {
            ImGui.Dummy(new Vector2(iconSize, iconSize));
        }
        finally {
            if (idPushed) ImGui.PopID();
        }
    }

    private void DrawVariationIndicator(ResolvedHotbarItem item, Vector2 drawPos, float iconSize, ImDrawListPtr drawList) {
        if (!item.HasVariations) return;

        ImGui.PushFont(UiBuilder.IconFont);
        var indicatorText = FontAwesomeIcon.Sync.ToIconString();
        var textSize = ImGui.CalcTextSize(indicatorText);
        ImGui.PopFont();

        var indicatorPos = new Vector2(drawPos.X + iconSize - textSize.X - 2f, drawPos.Y + iconSize - textSize.Y - 2f);
        drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), new Vector2(indicatorPos.X + 1, indicatorPos.Y + 1), ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f)), indicatorText);
        drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), indicatorPos, ImGui.GetColorU32(new Vector4(0.25f, 0.86f, 0.25f, 1f)), indicatorText);
    }

    private void DrawHotkeyOverlay(ResolvedHotbarItem item, Vector2 drawPos, ImDrawListPtr drawList) {
        var actionRef = new ActionReference {
            Type = item.EmoteId.HasValue ? ActionType.Emote : ActionType.Macro,
            EmoteId = item.EmoteId ?? 0,
            MacroId = item.MacroId ?? Guid.Empty
        };

        var assignedKey = this.hotkeyService.GetAssignedKey(actionRef);
        if (assignedKey == null) return;

        assignedKey.GetAbbreviatedFormat(out string modifiers, out string mainKey);

        var font = ImGui.GetFont();
        float normalFontSize = ImGui.GetFontSize();
        float smallFontSize = normalFontSize * 0.75f;

        // Remplacement de CalcTextSizeA par un calcul vectoriel linéaire supporté par l'API standard
        Vector2 modifiersSize = string.IsNullOrEmpty(modifiers) ? Vector2.Zero : ImGui.CalcTextSize(modifiers) * 0.75f;
        Vector2 mainKeySize = ImGui.CalcTextSize(mainKey);

        float totalWidth = modifiersSize.X + mainKeySize.X;
        float totalHeight = Math.Max(modifiersSize.Y, mainKeySize.Y);

        var textPos = new Vector2(drawPos.X + 2f, drawPos.Y + 2f);
        var bgMax = textPos + new Vector2(totalWidth, totalHeight) + new Vector2(4f, 2f);

        drawList.AddRectFilled(textPos, bgMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.7f)), 2f);

        float currentX = textPos.X + 2f;
        if (!string.IsNullOrEmpty(modifiers)) {
            float offsetY = mainKeySize.Y - modifiersSize.Y;
            drawList.AddText(font, smallFontSize, new Vector2(currentX, textPos.Y + 1f + offsetY), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)), modifiers);
            currentX += modifiersSize.X;
        }

        drawList.AddText(font, normalFontSize, new Vector2(currentX, textPos.Y + 1f), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)), mainKey);
    }
}