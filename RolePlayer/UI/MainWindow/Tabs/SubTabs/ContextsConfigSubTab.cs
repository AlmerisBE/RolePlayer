namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class ContextsConfigSubTab {
    private IContextManagementService contextService;
    private ILocalizationService localization;

    private string newContextName = string.Empty;
    private Guid cloneSourceId = Guid.Empty;
    private string editingContext = string.Empty;
    private string editName = string.Empty;
    private Guid contextToDelete = Guid.Empty;
    private bool isDeleteDialogOpen = false;

    public ContextsConfigSubTab(IContextManagementService contextService, ILocalizationService localization) {
        this.contextService = contextService;
        this.localization = localization;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_ctx_create"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float btnWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float remainingWidth = availableWidth - btnWidth - (spacing * 2);

        ImGui.SetNextItemWidth(remainingWidth * 0.5f);
        ImGui.InputTextWithHint("##NewContextName", this.localization.Translate("config_ctx_name_hint"), ref this.newContextName, 64);
        ImGui.SameLine();

        ImGui.SetNextItemWidth(remainingWidth * 0.5f);
        if (ImGui.BeginCombo("##CloneSource", this.cloneSourceId == Guid.Empty ? this.localization.Translate("config_ctx_clone_none") : this.contextService.GetAllContexts().First(c => c.Id == this.cloneSourceId).Name)) {
            if (ImGui.Selectable(this.localization.Translate("config_ctx_clone_empty"), this.cloneSourceId == Guid.Empty)) this.cloneSourceId = Guid.Empty;

            ImGui.Separator();
            foreach (var ctx in this.contextService.GetAllContexts()) {
                if (ImGui.Selectable(ctx.Name, this.cloneSourceId == ctx.Id)) this.cloneSourceId = ctx.Id;
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##CreateCtx", new Vector2(btnWidth, 0)) && !string.IsNullOrWhiteSpace(this.newContextName)) {
            this.contextService.CreateContext(this.newContextName, this.cloneSourceId == Guid.Empty ? null : this.cloneSourceId);
            this.newContextName = string.Empty;
            this.cloneSourceId = Guid.Empty;
        }
        ImGui.PopFont();
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_ctx_tooltip_add"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var contexts = this.contextService.GetAllContexts().ToList();
        var currentId = this.contextService.GetCurrentContext().Id;

        if (ImGui.BeginTable("ContextsTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_ctx_col_active"), ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 75f);
            ImGui.TableHeadersRow();

            foreach (var ctx in contexts) {
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 28f);

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                if (ImGui.RadioButton($"##SelCtx_{ctx.Id}", currentId == ctx.Id)) this.contextService.SwitchContext(ctx.Id);

                if (this.editingContext == ctx.Id.ToString()) {
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(-1f);
                    ImGui.InputText($"##EditCtxName_{ctx.Id}", ref this.editName, 64);

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Save.ToIconString()}##Save_{ctx.Id}")) {
                        this.contextService.RenameContext(ctx.Id, this.editName);
                        this.editingContext = string.Empty;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"{FontAwesomeIcon.Times.ToIconString()}##Cancel_{ctx.Id}")) this.editingContext = string.Empty;

                    ImGui.PopFont();
                }
                else {
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    if (currentId == ctx.Id) ImGui.TextColored(new Vector4(0.4f, 0.8f, 0.4f, 1.0f), ctx.Name);
                    else ImGui.Text(ctx.Name);

                    ImGui.TableNextColumn();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Edit.ToIconString()}##Edit_{ctx.Id}")) {
                        this.editingContext = ctx.Id.ToString();
                        this.editName = ctx.Name;
                    }
                    ImGui.SameLine();

                    if (contexts.Count > 1) {
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##Del_{ctx.Id}")) {
                            this.contextToDelete = ctx.Id;
                            this.isDeleteDialogOpen = true;
                        }
                        ImGui.PopStyleColor();
                    }
                    ImGui.PopFont();
                }
            }
            ImGui.EndTable();
        }

        this.DrawDeleteConfirmationModal();
    }

    private void DrawDeleteConfirmationModal() {
        if (this.isDeleteDialogOpen) {
            ImGui.OpenPopup(this.localization.Translate("config_ctx_del_title"));
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal(this.localization.Translate("config_ctx_del_title"), ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            var ctxName = this.contextService.GetAllContexts().FirstOrDefault(c => c.Id == this.contextToDelete)?.Name ?? "Unknown";
            ImGui.Text(this.localization.Translate("config_ctx_del_desc", ctxName));
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), this.localization.Translate("config_ctx_del_warn"));
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.localization.Translate("config_common_yes_delete"), new Vector2(120, 0))) {
                this.contextService.DeleteContext(this.contextToDelete);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
}