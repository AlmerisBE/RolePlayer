namespace RolePlayer.Core.Configuration.Services;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
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
        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return this.defaultProfile;
        }

        var profileId = $"{localPlayer.Name.TextValue}@{localPlayer.HomeWorld.RowId}";

        if (!this.config.Profiles.ContainsKey(profileId)) {
            this.config.Profiles[profileId] = new CharacterProfile();
            this.Save();
        }

        return this.config.Profiles[profileId];
    }

    public void Save() => this.pluginInterface.SavePluginConfig(this.config);
}