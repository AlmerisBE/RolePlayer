namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;

public interface IGameTemplateProvider {
    IReadOnlyDictionary<string, GameDefinition> GetDefaultTemplates();
}