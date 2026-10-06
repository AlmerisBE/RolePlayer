namespace RolePlayer.Core.Macros;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Parsers;
using RolePlayer.Core.Macros.Engine.Services;
using RolePlayer.Core.Macros.Services;

public class MacrosFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IMacroManagementService, MacroManagementService>();
        services.AddSingleton<IMacroExecutionService, MacroExecutionService>();

        // Injection du nouveau moteur de macros
        services.AddSingleton<IMacroCompiler, MacroCompiler>();
        services.AddSingleton<IMacroEngine, MacroEngine>();
        services.AddSingleton<ISystemVariableProvider, SystemVariableProvider>();

        // Enregistrement des stratégies de parsing (OCP)
        services.AddSingleton<IInstructionParser, WaitParser>();
        services.AddSingleton<IInstructionParser, LabelParser>();
        services.AddSingleton<IInstructionParser, GotoParser>();
        services.AddSingleton<IInstructionParser, SetVariableParser>();
        services.AddSingleton<IInstructionParser, IfParser>();
        services.AddSingleton<IInstructionParser, ElseParser>();
        services.AddSingleton<IInstructionParser, CallMacroParser>();
        services.AddSingleton<IInstructionParser, StopParser>();
    }
}