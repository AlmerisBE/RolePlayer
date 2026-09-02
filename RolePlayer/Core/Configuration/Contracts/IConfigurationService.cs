using RolePlayer.Core.Configuration.Models;

namespace RolePlayer.Core.Configuration.Contracts;

public interface IConfigurationService {
    PluginConfiguration GetConfig();
    void Save();
}