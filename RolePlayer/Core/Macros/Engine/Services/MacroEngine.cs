namespace RolePlayer.Core.Macros.Engine.Services;

using Dalamud.Plugin.Services;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Models;
using RolePlayer.Core.Macros.Models;
using System;
using System.Numerics;

public class MacroEngine : IMacroEngine, IDisposable {
    private IMacroCompiler compiler;
    private ISystemVariableProvider systemVariableProvider;
    private IFramework framework;
    private IObjectTable objectTable;
    private ILoggerService logger;

    private MacroExecutionContext? currentContext;

    public bool IsRunning => this.currentContext != null && this.currentContext.State != MacroExecutionState.Finished && this.currentContext.State != MacroExecutionState.Error;

    public MacroEngine(
        IMacroCompiler compiler,
        ISystemVariableProvider systemVariableProvider,
        IFramework framework,
        IObjectTable objectTable,
        ILoggerService logger) {

        this.compiler = compiler;
        this.systemVariableProvider = systemVariableProvider;
        this.framework = framework;
        this.objectTable = objectTable;
        this.logger = logger;
    }

    public void Play(RoleplayMacro macro) {
        if (this.IsRunning) {
            this.logger.Warning("A macro is already running. Ignoring new request.");
            return;
        }

        if (!this.compiler.TryCompile(macro, out var frame, out string error) || frame == null) {
            this.logger.Error($"Failed to compile macro '{macro.Name}': {error}");
            return;
        }

        this.currentContext = new MacroExecutionContext {
            RootMacroId = macro.Id,
            CancelOnMove = true
        };

        var player = this.objectTable.LocalPlayer;
        if (player != null) this.currentContext.LastKnownPosition = player.Position;

        // Hydratation des variables système (Job, Cible, etc.)
        this.systemVariableProvider.HydrateContext(this.currentContext);

        this.currentContext.CallStack.Push(frame);
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public void Stop() {
        if (this.currentContext != null) this.currentContext.State = MacroExecutionState.Finished;
        this.framework.Update -= this.OnFrameworkUpdate;
        this.currentContext = null;
    }

    private void OnFrameworkUpdate(Dalamud.Plugin.Services.IFramework fw) {
        if (this.currentContext == null || this.currentContext.State == MacroExecutionState.Finished || this.currentContext.State == MacroExecutionState.Error) {
            this.Stop();
            return;
        }

        this.HandleMovementSuspension();

        if (this.currentContext.State == MacroExecutionState.Suspended) return;
        if (DateTime.Now < this.currentContext.ResumeTime) return;

        int executionLimit = 10;
        int executed = 0;

        while (this.currentContext.State == MacroExecutionState.Running && DateTime.Now >= this.currentContext.ResumeTime && executed < executionLimit) {
            if (this.currentContext.CallStack.Count == 0) {
                this.currentContext.State = MacroExecutionState.Finished;
                break;
            }

            var currentFrame = this.currentContext.CallStack.Peek();
            if (currentFrame.ProgramCounter >= currentFrame.Instructions.Count) {
                this.currentContext.CallStack.Pop();
                continue;
            }

            var instruction = currentFrame.Instructions[currentFrame.ProgramCounter];
            currentFrame.ProgramCounter++;

            try {
                instruction.Execute(this.currentContext);
            }
            catch (Exception ex) {
                this.logger.Error(ex, "Error executing macro instruction.");
                this.currentContext.State = MacroExecutionState.Error;
            }

            executed++;

            // Interrompt la boucle synchrone pour cette frame, permettant au jeu de traiter l'action
            if (this.currentContext.YieldFrame) {
                this.currentContext.YieldFrame = false;
                break;
            }
        }
    }

    private void HandleMovementSuspension() {
        if (!this.currentContext!.CancelOnMove) return;

        var player = this.objectTable.LocalPlayer;
        if (player == null) return;

        var currentPos = player.Position;
        float distance = Vector3.Distance(currentPos, this.currentContext.LastKnownPosition);

        if (distance > 0.05f) {
            if (this.currentContext.State == MacroExecutionState.Running) {
                this.currentContext.State = MacroExecutionState.Suspended;
            }
            this.currentContext.LastKnownPosition = currentPos;
            this.currentContext.LastMovementTime = DateTime.Now;
        }
        else if (this.currentContext.State == MacroExecutionState.Suspended) {
            if ((DateTime.Now - this.currentContext.LastMovementTime).TotalSeconds > 0.5) {
                this.currentContext.State = MacroExecutionState.Running;
            }
        }
    }

    public void Dispose() => this.Stop();
}