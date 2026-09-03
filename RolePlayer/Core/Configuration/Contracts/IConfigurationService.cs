namespace RolePlayer.Core.Configuration.Contracts;

using RolePlayer.Core.Configuration.Models;

public interface IConfigurationService {
    PluginConfiguration GetConfig();
    CharacterProfile GetCurrentProfile();
    void Save();
}