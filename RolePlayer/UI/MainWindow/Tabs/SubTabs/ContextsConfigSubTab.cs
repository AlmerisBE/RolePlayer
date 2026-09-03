namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class ContextsConfigSubTab {
    private IContextManagementService contextService;

    private string newContextName = string.Empty;
    private Guid cloneSourceId = Guid.Empty;
    private string editingContext = string.Empty;
    private string editName = string.Empty;
    private Guid contextToDelete = Guid.Empty;
    private bool isDeleteDialogOpen = false;

    public ContextsConfigSubTab(IContextManagementService contextService) {
        this.contextService = contextService;
    }

    public void Draw() {
        ImGui.Text("Create New Context");

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float btnWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float remainingWidth = availableWidth - btnWidth - (spacing * 2);

        ImGui.SetNextItemWidth(remainingWidth * 0.5f);
        ImGui.InputTextWithHint("##NewContextName", "Context Name", ref this.newContextName, 64);
        ImGui.SameLine();

        ImGui.SetNextItemWidth(remainingWidth * 0.5f);
        if (ImGui.BeginCombo("##CloneSource", this.cloneSourceId == Guid.Empty ? "Clone from (None)" : this.contextService.GetAllContexts().First(c => c.Id == this.cloneSourceId).Name)) {
            if (ImGui.Selectable("None (Empty Context)", this.cloneSourceId == Guid.Empty)) {
                this.cloneSourceId = Guid.Empty;
            }

            ImGui.Separator();
            foreach (var ctx in this.contextService.GetAllContexts()) {
                if (ImGui.Selectable(ctx.Name, this.cloneSourceId == ctx.Id)) {
                    this.cloneSourceId = ctx.Id;
                }
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
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Add Context");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var contexts = this.contextService.GetAllContexts().ToList();
        var currentId = this.contextService.GetCurrentContext().Id;

        if (ImGui.BeginTable("ContextsTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn("Active", ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 75f);
            ImGui.TableHeadersRow();

            foreach (var ctx in contexts) {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                // Suppression du texte, seul le bouton radio est affiché
                if (ImGui.RadioButton($"##SelCtx_{ctx.Id}", currentId == ctx.Id)) {
                    this.contextService.SwitchContext(ctx.Id);
                }

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
                    if (ImGui.Button($"{FontAwesomeIcon.Times.ToIconString()}##Cancel_{ctx.Id}")) {
                        this.editingContext = string.Empty;
                    }

                    ImGui.PopFont();
                }
                else {
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    if (currentId == ctx.Id) {
                        ImGui.TextColored(new Vector4(0.4f, 0.8f, 0.4f, 1.0f), ctx.Name);
                    }
                    else {
                        ImGui.Text(ctx.Name);
                    }

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
            ImGui.OpenPopup("Delete Context Confirmation");
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal("Delete Context Confirmation", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            var ctxName = this.contextService.GetAllContexts().FirstOrDefault(c => c.Id == this.contextToDelete)?.Name ?? "Unknown";
            ImGui.Text($"Are you sure you want to delete the context '{ctxName}'?");
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), "All hotbars, tags, and groups within this context will be permanently lost.");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Yes, Delete", new Vector2(120, 0))) {
                this.contextService.DeleteContext(this.contextToDelete);
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0))) {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
}