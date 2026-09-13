namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class DefaultGameTemplateProvider : IGameTemplateProvider {
    private IEnumerable<IDefaultGameTemplate> templates;

    public DefaultGameTemplateProvider(IEnumerable<IDefaultGameTemplate> templates) {
        this.templates = templates;
    }

    public IReadOnlyDictionary<string, GameDefinition> GetDefaultTemplates() {
        var result = new Dictionary<string, GameDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var template in this.templates) {
            result[template.FileName] = template.Build();
        }

        return result;
    }
}