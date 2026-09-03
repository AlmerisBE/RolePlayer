namespace RolePlayer.Core.Configuration.Services;

using Dalamud.Plugin;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using System.Collections.Generic;

public class ConfigurationService : IConfigurationService {
    private IDalamudPluginInterface pluginInterface;
    private PluginConfiguration config;

    public ConfigurationService(IDalamudPluginInterface pluginInterface) {
        this.pluginInterface = pluginInterface;

        this.config = this.pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

        // Defensive instantiation: Newtonsoft.Json can overwrite defaults with null during deserialization of legacy configurations
        if (this.config.EmoteGroups == null) {
            this.config.EmoteGroups = new();
        }

        if (this.config.EmoteTags == null) {
            this.config.EmoteTags = new();
        }

        if (this.config.EmoteToGroupMap == null) {
            this.config.EmoteToGroupMap = new();
        }

        // Ensure AvailableTags is instantiated if it was recently added
        if (this.config.AvailableTags == null) {
            this.config.AvailableTags = new HashSet<string>();
        }
    }

    public PluginConfiguration GetConfig() => this.config;

    public void Save() {
        this.pluginInterface.SavePluginConfig(this.config);
    }
}