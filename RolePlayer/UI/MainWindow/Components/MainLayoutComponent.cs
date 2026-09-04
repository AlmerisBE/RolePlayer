namespace RolePlayer.UI.MainWindow.Components;

using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

public class MainLayoutComponent {
    private const float BaseWidth = 400f;
    private const float SidePanelWidth = 300f;
    private bool lastPanelState = false;
    private bool isFirstFrame = true;

    public void Draw(Action drawMainContent, Action drawSidePanel, bool isPanelOpen) {
        var panelTotalWidth = SidePanelWidth + ImGui.GetStyle().ItemSpacing.X;

        if (this.isFirstFrame) {
            var initialSize = ImGui.GetWindowSize();
            if (!isPanelOpen && initialSize.X >= BaseWidth + panelTotalWidth - 20f) {
                ImGui.SetWindowSize(new Vector2(initialSize.X - panelTotalWidth, initialSize.Y));
            }

            this.isFirstFrame = false;
            this.lastPanelState = isPanelOpen;
        }
        else if (isPanelOpen != this.lastPanelState) {
            var currentSize = ImGui.GetWindowSize();
            var targetWidth = isPanelOpen ? currentSize.X + panelTotalWidth : currentSize.X - panelTotalWidth;
            if (targetWidth < BaseWidth) {
                targetWidth = BaseWidth;
            }

            ImGui.SetWindowSize(new Vector2(targetWidth, currentSize.Y));
            this.lastPanelState = isPanelOpen;
        }

        var contentWidth = isPanelOpen ? -panelTotalWidth : 0;

        var footerHeight = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y;

        if (ImGui.BeginChild("MainContent", new Vector2(contentWidth, -footerHeight), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)) {
            drawMainContent();
        }

        ImGui.EndChild();

        if (isPanelOpen) {
            ImGui.SameLine();
            if (ImGui.BeginChild("SidePanel", new Vector2(SidePanelWidth, -footerHeight), true)) {
                drawSidePanel();
            }

            ImGui.EndChild();
        }
    }
}