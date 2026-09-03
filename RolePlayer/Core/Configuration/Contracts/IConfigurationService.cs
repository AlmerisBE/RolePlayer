namespace RolePlayer.Core.Configuration.Contracts;

using RolePlayer.Core.Configuration.Models;
using System;

public interface IConfigurationService {
    event Action ProfileLoaded;
    PluginConfiguration GetConfig();
    CharacterProfile GetCurrentProfile();
    void Save();
}