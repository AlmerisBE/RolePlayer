namespace RolePlayer.UI.Hotbar.Services;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Windows;
using System;

public class HotbarWindowFactory : IHotbarWindowFactory {
    private IServiceProvider serviceProvider;

    public HotbarWindowFactory(IServiceProvider serviceProvider) {
        this.serviceProvider = serviceProvider;
    }

    public HotbarWindow Create(HotbarConfig config) {
        return ActivatorUtilities.CreateInstance<HotbarWindow>(this.serviceProvider, config);
    }
}