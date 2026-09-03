namespace RolePlayer.Core.Configuration.Services;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using System;
using System.Collections.Generic;

public class ConfigurationService : IConfigurationService, IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IObjectTable objectTable;
    private IFramework framework;

    private PluginConfiguration config;
    private CharacterProfile defaultProfile = new();
    private string currentProfileId = string.Empty;

    public event Action? ProfileLoaded;

    public ConfigurationService(IDalamudPluginInterface pluginInterface, IObjectTable objectTable, IFramework framework) {
        this.pluginInterface = pluginInterface;
        this.objectTable = objectTable;
        this.framework = framework;

        this.config = this.pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        if (this.config.Profiles == null) {
            this.config.Profiles = new Dictionary<string, CharacterProfile>();
        }

        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        try {
            var localPlayer = this.objectTable.LocalPlayer;
            if (localPlayer == null || localPlayer.Name == null) {
                if (this.currentProfileId != string.Empty) {
                    this.currentProfileId = string.Empty;
                    this.ProfileLoaded?.Invoke();
                }
                return;
            }

            var name = localPlayer.Name.TextValue;
            if (string.IsNullOrEmpty(name)) {
                return;
            }

            var worldId = localPlayer.HomeWorld.RowId;
            if (worldId == 0) {
                return;
            }

            var profileId = $"{name}@{worldId}";

            if (this.currentProfileId != profileId) {
                this.currentProfileId = profileId;
                this.ProfileLoaded?.Invoke();
            }
        }
        catch (Exception) {
            if (this.currentProfileId != string.Empty) {
                this.currentProfileId = string.Empty;
                this.ProfileLoaded?.Invoke();
            }
        }
    }

    public PluginConfiguration GetConfig() => this.config;

    public CharacterProfile GetCurrentProfile() {
        if (string.IsNullOrEmpty(this.currentProfileId)) {
            return this.defaultProfile;
        }

        if (!this.config.Profiles.ContainsKey(this.currentProfileId)) {
            this.config.Profiles[this.currentProfileId] = new CharacterProfile();
            this.Save();
        }

        return this.config.Profiles[this.currentProfileId];
    }

    public void Save() => this.pluginInterface.SavePluginConfig(this.config);

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}