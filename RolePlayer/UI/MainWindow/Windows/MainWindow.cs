namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public class MainWindow : Window, IDisposable {
    private IEnumerable<IEmoteBrowserTab> tabs;
    private EmoteDetailsPanel detailsPanel;
    private IEmoteSelectionState selectionState;
    private IClientState clientState;
    private IEmoteExecutionService executionService;
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;

    private const float BaseWidth = 400f;
    private const float SidePanelWidth = 300f;
    private bool lastPanelState = false;
    private bool isFirstFrame = true;

    private int unlockedEmotesCount = 0;
    private int totalEmotesCount = 0;

    public MainWindow(
        IDalamudPluginInterface pluginInterface,
        IEnumerable<IEmoteBrowserTab> tabs,
        EmoteDetailsPanel detailsPanel,
        IEmoteSelectionState selectionState,
        IClientState clientState,
        IEmoteExecutionService executionService,
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider)
        : base($"RolePlayer v{pluginInterface.Manifest.AssemblyVersion}", ImGuiWindowFlags.None) {

        this.tabs = tabs.OrderBy(t => t.SortOrder).ToList();
        this.detailsPanel = detailsPanel;
        this.selectionState = selectionState;
        this.clientState = clientState;
        this.executionService = executionService;
        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;

        this.clientState.Logout += this.OnLogout;
        this.clientState.Login += this.OnLogin;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(BaseWidth, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.CalculateEmoteStatsAsync();
    }

    private void CalculateEmoteStatsAsync() {
        Task.Run(() => {
            var emotes = this.emoteRepository.GetBaseEmotes().ToList();
            this.totalEmotesCount = emotes.Count;
            this.unlockedEmotesCount = emotes.Count(e => !e.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(e.Id));
        });
    }

    public override void Draw() {
        var isPanelOpen = this.selectionState.SelectedEmote != null;

        if (this.isFirstFrame) {
            var initialSize = ImGui.GetWindowSize();
            if (!isPanelOpen && initialSize.X >= BaseWidth + SidePanelWidth - 20f) {
                ImGui.SetWindowSize(new Vector2(initialSize.X - SidePanelWidth, initialSize.Y));
            }

            this.isFirstFrame = false;
            this.lastPanelState = isPanelOpen;
        }
        else if (isPanelOpen != this.lastPanelState) {
            var currentSize = ImGui.GetWindowSize();
            var targetWidth = isPanelOpen ? currentSize.X + SidePanelWidth : currentSize.X - SidePanelWidth;
            if (targetWidth < BaseWidth) {
                targetWidth = BaseWidth;
            }

            ImGui.SetWindowSize(new Vector2(targetWidth, currentSize.Y));
            this.lastPanelState = isPanelOpen;
        }

        var contentWidth = isPanelOpen ? -(SidePanelWidth + ImGui.GetStyle().ItemSpacing.X) : 0;
        var footerHeight = ImGui.GetFrameHeightWithSpacing();

        if (ImGui.BeginChild("MainContent", new Vector2(contentWidth, -footerHeight), false)) {
            this.DrawTabs();
        }
        ImGui.EndChild();

        if (isPanelOpen) {
            ImGui.SameLine();
            if (ImGui.BeginChild("SidePanel", new Vector2(SidePanelWidth, -footerHeight), true)) {
                this.detailsPanel.Draw();
            }
            ImGui.EndChild();
        }

        ImGui.Separator();
        this.DrawStatusBar();
    }

    private void DrawTabs() {
        if (ImGui.BeginTabBar("MainTabBar", ImGuiTabBarFlags.Reorderable)) {
            foreach (var tab in this.tabs) {
                if (ImGui.BeginTabItem(tab.TabName)) {
                    tab.Draw();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    private void DrawStatusBar() {
        if (ImGui.BeginChild("StatusBar", new Vector2(0, 0), false, ImGuiWindowFlags.NoScrollbar)) {
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.UserFriends.ToIconString())) {
                this.executionService.OpenNativeEmoteWindow();
            }

            ImGui.PopFont();

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Ouvrir la fenêtre des emotes du jeu");
            }

            var statsText = $"{this.unlockedEmotesCount} / {this.totalEmotesCount} débloquées";
            // 25f de marge pour éviter l'enchevêtrement avec le grip de redimensionnement de la fenêtre
            var textSize = ImGui.CalcTextSize(statsText).X + 25f;

            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - textSize);
            ImGui.AlignTextToFramePadding();
            // Retrait de TextDisabled pour un affichage blanc standard
            ImGui.Text(statsText);
        }
        ImGui.EndChild();
    }

    private void OnLogout(int type, int code) {
        this.selectionState.SelectedEmote = null;
    }

    private void OnLogin() {
        this.CalculateEmoteStatsAsync();
    }

    public void Dispose() {
        this.clientState.Logout -= this.OnLogout;
        this.clientState.Login -= this.OnLogin;
    }
}