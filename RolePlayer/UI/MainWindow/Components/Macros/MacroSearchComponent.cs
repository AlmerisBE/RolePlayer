namespace RolePlayer.UI.MainWindow.Components.Macros;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.Localization.Contracts;

public class MacroSearchComponent {
    private ILocalizationService localization;

    public string Query { get; set; } = string.Empty;

    public MacroSearchComponent(ILocalizationService localization) {
        this.localization = localization;
    }

    public void Draw() {
        var query = this.Query;
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.InputTextWithHint("##MacroSearch", this.localization.Translate("config_macro_search_hint"), ref query, 128)) {
            this.Query = query;
        }
    }
}