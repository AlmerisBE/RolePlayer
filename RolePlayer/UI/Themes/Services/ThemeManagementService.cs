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

        var path = Path.Combine(this.ThemeDirectory, "FFXIV_Dark.json");
        if (File.Exists(path)) return;

        var defaultTheme = new RolePlayerTheme {
            Name = "FFXIV Dark",
            Colors = new Dictionary<string, string> {
                { "WindowBg", "#261C14F2" },
                { "TitleBg", "#3A2A20FF" },
                { "TitleBgActive", "#5A4232FF" },
                { "Button", "#503D2EFF" },
                { "ButtonHovered", "#6D5541FF" },
                { "ButtonActive", "#8A6D55FF" },
                { "FrameBg", "#1E1510FF" },
                { "FrameBgHovered", "#2D2018FF" },
                { "FrameBgActive", "#402E24FF" },
                { "Text", "#D4C7B8FF" }
            }
        };

        try {
            File.WriteAllText(path, JsonSerializer.Serialize(defaultTheme, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Failed to write the default FFXIV Dark theme file.");
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
        foreach (var kvp in this.currentThemeColors) ImGui.PushStyleColor(kvp.Key, kvp.Value);
    }

    public void PopTheme() {
        if (this.currentThemeColors.Count > 0) ImGui.PopStyleColor(this.currentThemeColors.Count);
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