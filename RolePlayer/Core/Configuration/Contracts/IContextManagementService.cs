namespace RolePlayer.Core.Configuration.Contracts;

using RolePlayer.Core.Configuration.Models;
using System;
using System.Collections.Generic;

public interface IContextManagementService {
    event Action ContextChanged;
    EmoteContext GetCurrentContext();
    IEnumerable<EmoteContext> GetAllContexts();
    void CreateContext(string name, Guid? cloneFromId);
    void SwitchContext(Guid contextId);
    void SwitchContextByName(string name);
    void RenameContext(Guid contextId, string newName);
    void DeleteContext(Guid contextId);
}