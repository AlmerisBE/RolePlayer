namespace RolePlayer.UI.Common.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.UI.Common.Contracts;
using RolePlayer.UI.Localization.Contracts;

public class HelpMarkerComponent : IHelpMarkerComponent {
    private ILocalizationService localization;

    public HelpMarkerComponent(ILocalizationService localization) {
        this.localization = localization;
    }

    public void Draw(string localizationKey) {
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextDisabled(FontAwesomeIcon.InfoCircle.ToIconString());
        ImGui.PopFont();

        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(this.localization.Translate(localizationKey));
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }
}