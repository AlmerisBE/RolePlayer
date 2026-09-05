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

        this.EnsureDirectoryAndDefaultTheme();
        this.LoadTheme(this.configurationService.GetConfig().SelectedTheme);
    }

    private void EnsureDirectoryAndDefaultTheme() {
        if (!Directory.Exists(this.ThemeDirectory)) Directory.CreateDirectory(this.ThemeDirectory);

        var path = Path.Combine(this.ThemeDirectory, "Dark.json");
        if (File.Exists(path)) return;

        var defaultTheme = new RolePlayerTheme {
            Name = "Dark",
            Colors = new Dictionary<string, string> {
                { "WindowBg", "#111111FF" },
                { "TitleBg", "#1A1A1AFF" },
                { "TitleBgActive", "#2B2B2BFF" },
                { "Button", "#333333FF" },
                { "ButtonHovered", "#444444FF" },
                { "ButtonActive", "#555555FF" },
                { "FrameBg", "#222222FF" },
                { "FrameBgHovered", "#333333FF" },
                { "FrameBgActive", "#444444FF" },
                { "Text", "#EEEEEEFF" },
                { "Header", "#333333FF" },
                { "HeaderHovered", "#444444FF" },
                { "HeaderActive", "#555555FF" },
                { "Tab", "#222222FF" },
                { "TabHovered", "#444444FF" },
                { "TabActive", "#333333FF" }
            }
        };

        try {
            File.WriteAllText(path, JsonSerializer.Serialize(defaultTheme, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Failed to write the default Dark theme file.");
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

            if (theme == null || theme.Colors == null) return;

            foreach (var kvp in theme.Colors) {
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