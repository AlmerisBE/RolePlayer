namespace RolePlayer.UI.Themes.Services;

using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.Themes.Contracts;
using RolePlayer.UI.Themes.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

public class ThemeManagementService : IThemeManagementService {
    private IDalamudPluginInterface pluginInterface;
    private IConfigurationService configurationService;
    private ILoggerService logger;

    private Dictionary<ImGuiCol, Vector4> currentThemeColors = new();
    private int pushedColorsCount = 0;

    public string ThemeDirectory => Path.Combine(this.pluginInterface.ConfigDirectory.FullName, "Themes");

    public ThemeManagementService(IDalamudPluginInterface pluginInterface, IConfigurationService configurationService, ILoggerService logger) {
        this.pluginInterface = pluginInterface;
        this.configurationService = configurationService;
        this.logger = logger;

        this.EnsureDirectoryAndDefaultThemes();
        this.LoadTheme(this.configurationService.GetConfig().SelectedTheme);
    }

    private void EnsureDirectoryAndDefaultThemes() {
        if (!Directory.Exists(this.ThemeDirectory)) Directory.CreateDirectory(this.ThemeDirectory);

        var darkPath = Path.Combine(this.ThemeDirectory, "Dark.json");
        if (!File.Exists(darkPath)) {
            var darkTheme = new RolePlayerTheme {
                Name = "Dark",
                Author = "Almeris",
                Palette = new Dictionary<string, string> {
                    { "WindowBg", "#262323F2" },
                    { "Text", "#E5E5E5FF" },
                    { "ChildBg", "#1E1C1C7F" },
                    { "PopupBg", "#262323F2" },
                    { "FrameBg", "#333333FF" },
                    { "FrameBgHovered", "#3F3F3FFF" },
                    { "FrameBgActive", "#4C4C4CFF" },
                    { "TitleBg", "#1E1C1CFF" },
                    { "TitleBgActive", "#332626FF" },
                    { "TitleBgCollapsed", "#191919FF" },
                    { "TableHeaderBg", "#2D2B2BFF" },
                    { "TableRowBg", "#262323FF" },
                    { "TableRowBgAlt", "#2D2B2BFF" },
                    { "Border", "#4C3F3FFF" },
                    { "Tab", "#262323FF" },
                    { "TabHovered", "#3F3333FF" },
                    { "TabActive", "#4C3F3FFF" },
                    { "TabUnfocused", "#1E1C1CFF" },
                    { "TabUnfocusedActive", "#2D2B2BFF" },
                    { "Button", "#3F3333FF" },
                    { "ButtonHovered", "#593F3FFF" },
                    { "ButtonActive", "#664C4CFF" }
                }
            };

            try {
                File.WriteAllText(darkPath, JsonSerializer.Serialize(darkTheme, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) {
                this.logger.Error(ex, "Failed to write the default Dark theme file.");
            }
        }

        var lightPath = Path.Combine(this.ThemeDirectory, "Light.json");
        if (!File.Exists(lightPath)) {
            var lightTheme = new RolePlayerTheme {
                Name = "Light",
                Author = "Almeris",
                Palette = new Dictionary<string, string> {
                    { "WindowBg", "#E8DBC4F9" },
                    { "Text", "#261C11FF" },
                    { "ChildBg", "#E0D1BA7F" },
                    { "PopupBg", "#E8DBC4F9" },
                    { "FrameBg", "#D8C6A5FF" },
                    { "FrameBgHovered", "#E5D1B2FF" },
                    { "FrameBgActive", "#CCB799FF" },
                    { "TitleBg", "#D8C6A5FF" },
                    { "TitleBgActive", "#E5D1B2FF" },
                    { "TitleBgCollapsed", "#CCB299FF" },
                    { "TableHeaderBg", "#D1BA99FF" },
                    { "TableRowBg", "#E8DBC4FF" },
                    { "TableRowBgAlt", "#D8C9AFFF" },
                    { "Border", "#AA8966FF" },
                    { "Tab", "#D8C6A5FF" },
                    { "TabHovered", "#E5D1B2FF" },
                    { "TabActive", "#F2E5CCFF" },
                    { "TabUnfocused", "#CCB799FF" },
                    { "TabUnfocusedActive", "#D8C6A5FF" },
                    { "Button", "#D8C6A5FF" },
                    { "ButtonHovered", "#E5D1B2FF" },
                    { "ButtonActive", "#CCB799FF" }
                }
            };

            try {
                File.WriteAllText(lightPath, JsonSerializer.Serialize(lightTheme, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) {
                this.logger.Error(ex, "Failed to write the default Light theme file.");
            }
        }
    }

    public IEnumerable<string> GetAvailableThemes() {
        if (!Directory.Exists(this.ThemeDirectory)) return Enumerable.Empty<string>();

        return Directory.GetFiles(this.ThemeDirectory, "*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n)!;
    }

    public void LoadTheme(string themeName) {
        this.currentThemeColors.Clear();

        if (string.IsNullOrEmpty(themeName) || themeName.Equals("Default", StringComparison.OrdinalIgnoreCase)) return;

        var path = Path.Combine(this.ThemeDirectory, $"{themeName}.json");
        if (!File.Exists(path)) return;

        try {
            var json = File.ReadAllText(path);
            var theme = JsonSerializer.Deserialize<RolePlayerTheme>(json);

            if (theme == null || theme.Palette == null) return;

            foreach (var kvp in theme.Palette) {
                if (Enum.TryParse<ImGuiCol>(kvp.Key, true, out var colEnum)) {
                    this.currentThemeColors[colEnum] = this.ParseHexColor(kvp.Value);
                }
            }
        }
        catch (Exception ex) {
            this.logger.Error(ex, $"Failed to load or parse theme file: {themeName}");
        }
    }

    public void PushTheme() {
        this.pushedColorsCount = this.currentThemeColors.Count;
        foreach (var kvp in this.currentThemeColors) ImGui.PushStyleColor(kvp.Key, kvp.Value);
    }

    public void PopTheme() {
        if (this.pushedColorsCount > 0) {
            ImGui.PopStyleColor(this.pushedColorsCount);
            this.pushedColorsCount = 0;
        }
    }

    public void OpenThemeDirectory() {
        if (!Directory.Exists(this.ThemeDirectory)) Directory.CreateDirectory(this.ThemeDirectory);

        Process.Start(new ProcessStartInfo {
            FileName = this.ThemeDirectory,
            UseShellExecute = true
        });
    }

    private Vector4 ParseHexColor(string hex) {
        if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#")) return Vector4.Zero;

        hex = hex.Substring(1);
        if (hex.Length == 6) hex += "FF";
        if (hex.Length != 8) return Vector4.Zero;

        try {
            float r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
            float g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
            float b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
            float a = Convert.ToInt32(hex.Substring(6, 2), 16) / 255f;
            return new Vector4(r, g, b, a);
        }
        catch {
            return Vector4.Zero;
        }
    }
}