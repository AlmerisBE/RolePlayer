namespace RolePlayer.Core.Configuration.UI;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using RolePlayer.Core.Configuration.Contracts;
using System.Numerics;

public class ConfigWindow : Window {
    private IConfigurationService configurationService;

    public ConfigWindow(IConfigurationService configurationService)
        : base("RolePlayer Configuration", ImGuiWindowFlags.None) {
        this.configurationService = configurationService;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(300, 150),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        ImGui.TextWrapped("Configuration options will be available here.");
    }
}