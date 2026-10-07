namespace RolePlayer.UI.MainWindow.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using RolePlayer.UI.Localization.Contracts;
using System.Numerics;

public class MacroGuideWindow : Window {
    private ILocalizationService localization;

    public MacroGuideWindow(ILocalizationService localization) : base("MacroGuideWindow##RolePlayer", ImGuiWindowFlags.NoCollapse) {
        this.localization = localization;
        this.Size = new Vector2(650, 550);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw() {
        this.WindowName = this.localization.Translate("macro_guide_title");
    }

    public override void Draw() {
        if (ImGui.BeginTabBar("GuideTabs")) {
            if (ImGui.BeginTabItem(this.localization.Translate("macro_guide_tab_overview"))) {
                this.DrawOverview();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(this.localization.Translate("macro_guide_tab_flow"))) {
                this.DrawControlFlow();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(this.localization.Translate("macro_guide_tab_vars"))) {
                this.DrawVariables();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawOverview() {
        ImGui.Spacing();
        ImGui.TextWrapped(this.localization.Translate("macro_guide_overview_desc"));
        ImGui.Spacing();

        ImGui.TextColored(ImGuiColors.DalamudYellow, this.localization.Translate("macro_guide_rules_title"));
        ImGui.BulletText(this.localization.Translate("macro_guide_rule_1"));
        ImGui.BulletText(this.localization.Translate("macro_guide_rule_2"));
        ImGui.BulletText(this.localization.Translate("macro_guide_rule_3"));

        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudYellow, this.localization.Translate("macro_guide_example_title"));
        ImGui.Indent();

        ImGui.TextColored(ImGuiColors.ParsedGreen, "# if {Emote.Unlocked./dance}\n/dance\n# else\n/em doesn't know how to dance.");

        ImGui.Unindent();
    }

    private void DrawControlFlow() {
        ImGui.Spacing();
        if (ImGui.BeginTable("FlowTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {
            ImGui.TableSetupColumn(this.localization.Translate("macro_guide_flow_directive"), ImGuiTableColumnFlags.WidthFixed, 150f);
            ImGui.TableSetupColumn(this.localization.Translate("macro_guide_flow_desc"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            this.DrawTableRow("# wait [seconds]", this.localization.Translate("macro_guide_flow_wait"));
            this.DrawTableRow("# stop", this.localization.Translate("macro_guide_flow_stop"));
            this.DrawTableRow("# call [id]", this.localization.Translate("macro_guide_flow_call"));
            this.DrawTableRow("# if [condition]", this.localization.Translate("macro_guide_flow_if"));
            this.DrawTableRow("# else", this.localization.Translate("macro_guide_flow_else"));
            this.DrawTableRow("# goto [label]", this.localization.Translate("macro_guide_flow_goto"));
            this.DrawTableRow("# label [name]", this.localization.Translate("macro_guide_flow_label"));

            ImGui.EndTable();
        }
    }

    private void DrawVariables() {
        ImGui.Spacing();
        ImGui.TextWrapped(this.localization.Translate("macro_guide_vars_desc"));
        ImGui.Spacing();

        if (ImGui.BeginTable("VarTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {
            ImGui.TableSetupColumn(this.localization.Translate("macro_guide_vars_variable"), ImGuiTableColumnFlags.WidthFixed, 250f);
            ImGui.TableSetupColumn(this.localization.Translate("macro_guide_vars_desc_col"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            this.DrawTableRow("{Emote.Unlocked.[cmd]}", this.localization.Translate("macro_guide_vars_emote_unlocked"));
            this.DrawTableRow("{Emote.Active.[cmd]}", this.localization.Translate("macro_guide_vars_emote_active_cmd"));
            this.DrawTableRow("{Emote.Active}", this.localization.Translate("macro_guide_vars_emote_active_id"));
            this.DrawTableRow("{Player.Job}", this.localization.Translate("macro_guide_vars_player_job"));

            ImGui.EndTable();
        }
    }

    private void DrawTableRow(string col1, string col2) {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextColored(ImGuiColors.DalamudOrange, col1);
        ImGui.TableNextColumn();
        ImGui.TextWrapped(col2);
    }
}