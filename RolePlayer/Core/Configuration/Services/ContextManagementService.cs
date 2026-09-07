namespace RolePlayer.Core.Configuration.Services;

using Newtonsoft.Json;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class ContextManagementService : IContextManagementService {
    private IConfigurationService configService;

    public event Action? ContextChanged;

    public ContextManagementService(IConfigurationService configService) {
        this.configService = configService;
    }

    public EmoteContext GetCurrentContext() {
        var config = this.configService.GetConfig();
        var profile = this.configService.GetCurrentProfile();

        if (config.Contexts.TryGetValue(profile.ActiveContextId, out var context)) {
            return context;
        }

        var defaultContext = config.Contexts.Values.FirstOrDefault();
        if (defaultContext == null) {
            defaultContext = new EmoteContext();
            config.Contexts.Add(defaultContext.Id, defaultContext);
        }

        profile.ActiveContextId = defaultContext.Id;
        this.configService.Save();
        return defaultContext;
    }

    public IEnumerable<EmoteContext> GetAllContexts() {
        return this.configService.GetConfig().Contexts.Values.OrderBy(c => c.Name);
    }

    public void CreateContext(string name, Guid? cloneFromId) {
        if (string.IsNullOrWhiteSpace(name)) return;

        var config = this.configService.GetConfig();
        EmoteContext newContext;

        if (cloneFromId.HasValue && config.Contexts.TryGetValue(cloneFromId.Value, out var sourceContext)) {
            var serialized = JsonConvert.SerializeObject(sourceContext);
            newContext = JsonConvert.DeserializeObject<EmoteContext>(serialized) ?? new EmoteContext();
            newContext.Id = Guid.NewGuid();
            newContext.Name = name.Trim();

            foreach (var hotbar in newContext.Hotbars) {
                hotbar.Id = Guid.NewGuid();
            }
        }
        else {
            newContext = new EmoteContext { Name = name.Trim() };
        }

        config.Contexts.Add(newContext.Id, newContext);
        this.configService.Save();
    }

    public void SwitchContext(Guid contextId) {
        var config = this.configService.GetConfig();
        var profile = this.configService.GetCurrentProfile();

        if (!config.Contexts.ContainsKey(contextId) || profile.ActiveContextId == contextId) return;

        profile.ActiveContextId = contextId;
        this.configService.Save();
        this.ContextChanged?.Invoke();
    }

    public void SwitchContextByName(string name) {
        var context = this.GetAllContexts().FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (context != null) this.SwitchContext(context.Id);
    }

    public void RenameContext(Guid contextId, string newName) {
        if (string.IsNullOrWhiteSpace(newName)) return;

        var config = this.configService.GetConfig();
        if (config.Contexts.TryGetValue(contextId, out var context)) {
            context.Name = newName.Trim();
            this.configService.Save();
        }
    }

    public void DeleteContext(Guid contextId) {
        var config = this.configService.GetConfig();
        var profile = this.configService.GetCurrentProfile();

        if (config.Contexts.Count <= 1 || !config.Contexts.ContainsKey(contextId)) return;

        config.Contexts.Remove(contextId);

        if (profile.ActiveContextId == contextId) {
            profile.ActiveContextId = config.Contexts.Keys.First();
            this.ContextChanged?.Invoke();
        }

        this.configService.Save();
    }
}