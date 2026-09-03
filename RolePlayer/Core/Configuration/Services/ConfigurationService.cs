namespace RolePlayer.Core.Configuration.Services;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using System;
using System.Collections.Generic;

public class ConfigurationService : IConfigurationService {
    private IDalamudPluginInterface pluginInterface;
    private IObjectTable objectTable;
    private PluginConfiguration config;
    private CharacterProfile defaultProfile = new();

    public ConfigurationService(IDalamudPluginInterface pluginInterface, IObjectTable objectTable) {
        this.pluginInterface = pluginInterface;
        this.objectTable = objectTable;

        this.config = this.pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        if (this.config.Profiles == null) {
            this.config.Profiles = new Dictionary<string, CharacterProfile>();
        }
    }

    public PluginConfiguration GetConfig() => this.config;

    public CharacterProfile GetCurrentProfile() {
        try {
            var localPlayer = this.objectTable.LocalPlayer;
            if (localPlayer == null || localPlayer.Name == null) {
                return this.defaultProfile;
            }

            var name = localPlayer.Name.TextValue;
            if (string.IsNullOrEmpty(name)) {
                return this.defaultProfile;
            }

            // Utilisation de RowId selon l'implémentation de Lumina.Excel.RowRef dans l'API v10+
            var worldId = localPlayer.HomeWorld.RowId;
            if (worldId == 0) {
                return this.defaultProfile;
            }

            var profileId = $"{name}@{worldId}";

            if (!this.config.Profiles.ContainsKey(profileId)) {
                this.config.Profiles[profileId] = new CharacterProfile();
                this.Save();
            }

            return this.config.Profiles[profileId];
        }
        catch (Exception) {
            // Failsafe critique durant les écrans de chargement ou l'écran titre
            return this.defaultProfile;
        }
    }

    public void Save() => this.pluginInterface.SavePluginConfig(this.config);
}