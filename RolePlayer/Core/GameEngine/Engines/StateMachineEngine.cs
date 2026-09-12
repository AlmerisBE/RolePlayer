namespace RolePlayer.Core.GameEngine.Engines;

using Dalamud.Plugin.Services;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachineEngine : IGameEngine {
    private IConditionEvaluatorService conditionEvaluator;
    private IGameActionExecutionService actionExecutionService;
    private IFramework framework;

    private GameSessionConfig? config;
    private GameSessionContext context = new();
    private GameStage? currentStage;

    // Suivi des chronomètres actifs pour les TimerListener
    private Dictionary<GameModuleConfig, DateTime> activeTimers = new();

    public string EngineType => "StateMachineEngine";
    public bool IsRunning { get; private set; }

    public IReadOnlyList<string> Participants => this.context.Participants.ToList();
    public IReadOnlyDictionary<string, object> Variables => this.context.Variables;
    public string CurrentStageName => this.currentStage?.Name ?? "Unknown";

    public bool AllowChatRegistration { get; set; } = false;

    public event Action<string>? BroadcastRequested;
    public event Action? GameFinished;
    public event Action? ParticipantsChanged;
    public event Action? StageChanged;

    public StateMachineEngine(IConditionEvaluatorService conditionEvaluator, IGameActionExecutionService actionExecutionService, IFramework framework) {
        this.conditionEvaluator = conditionEvaluator;
        this.actionExecutionService = actionExecutionService;
        this.framework = framework;

        this.actionExecutionService.BroadcastRequested += msg => this.BroadcastRequested?.Invoke(msg);
        this.actionExecutionService.StageAdvanceRequested += this.AdvanceStage;
        this.actionExecutionService.GameStopRequested += this.Stop;
    }

    public void Initialize(GameSessionConfig config) {
        this.config = config;
    }

    public void Start() {
        if (this.config?.Game == null || this.config.Game.Stages.Count == 0) return;

        this.context.Variables.Clear();
        this.context.Participants.Clear();

        foreach (var kvp in this.config.Game.InitialVariables) {
            this.context.Variables[kvp.Key] = kvp.Value;
        }

        this.AllowChatRegistration = this.config.Game.AllowChatRegistration;
        this.IsRunning = true;
        this.framework.Update += this.OnFrameworkUpdate;

        this.SetStage(this.config.Game.Stages.First().Id);
    }

    public void Stop() {
        if (!this.IsRunning) return;
        this.IsRunning = false;
        this.framework.Update -= this.OnFrameworkUpdate;
        this.activeTimers.Clear();
        this.GameFinished?.Invoke();
    }

    public void SetVariable(string key, object value) {
        if (string.IsNullOrWhiteSpace(key)) return;
        this.context.Variables[key] = value;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (!this.IsRunning || this.currentStage == null || this.activeTimers.Count == 0) return;

        var triggeredModules = new List<GameModuleConfig>();

        foreach (var kvp in this.activeTimers.ToList()) {
            if (DateTime.Now >= kvp.Value) {
                triggeredModules.Add(kvp.Key);
                this.activeTimers.Remove(kvp.Key);
            }
        }

        if (triggeredModules.Count > 0) {
            foreach (var module in triggeredModules) {
                if (this.conditionEvaluator.EvaluateAll(module.ConditionExpressions, this.context)) {
                    this.actionExecutionService.ExecuteAll(module.OnTriggerActions, this.context);
                }
            }
            this.EvaluateTransitions("Auto");
        }
    }

    public void AddParticipant(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!this.context.Participants.Contains(name, StringComparer.OrdinalIgnoreCase)) {
            this.context.Participants.Add(name);
            this.ParticipantsChanged?.Invoke();
        }
    }

    public void RemoveParticipant(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;
        var index = this.context.Participants.FindIndex(p => p.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) {
            this.context.Participants.RemoveAt(index);
            this.ParticipantsChanged?.Invoke();
        }
    }

    public void AdvanceStage() {
        this.EvaluateTransitions("Manual");
    }

    public void ProcessEvent(GameEvent gameEvent) {
        if (!this.IsRunning || this.currentStage == null) return;

        this.context.CurrentEvent = gameEvent;

        foreach (var module in this.currentStage.ActiveModules) {
            if (!this.IsModuleTriggeredByEvent(module, gameEvent)) continue;

            if (this.conditionEvaluator.EvaluateAll(module.ConditionExpressions, this.context)) {
                this.actionExecutionService.ExecuteAll(module.OnTriggerActions, this.context);

                if (module.OnTriggerActions.Any(a => a.ActionType.Equals("RegisterPlayer", StringComparison.OrdinalIgnoreCase))) {
                    this.ParticipantsChanged?.Invoke();
                }
            }
        }

        this.EvaluateTransitions("OnEvent");
    }

    private void SetStage(string stageId) {
        var stage = this.config?.Game?.Stages.FirstOrDefault(s => s.Id.Equals(stageId, StringComparison.OrdinalIgnoreCase));
        if (stage == null) return;

        this.currentStage = stage;
        this.activeTimers.Clear();

        var timerModules = this.currentStage.ActiveModules.Where(m => m.ModuleType.Equals("TimerListener", StringComparison.OrdinalIgnoreCase));
        foreach (var mod in timerModules) {
            if (mod.Parameters.TryGetValue("DurationSeconds", out var durStr) && int.TryParse(durStr, out int duration)) {
                this.activeTimers[mod] = DateTime.Now.AddSeconds(duration);
            }
        }

        this.StageChanged?.Invoke();
        this.actionExecutionService.ExecuteAll(this.currentStage.OnEnterActions, this.context);
        this.EvaluateTransitions("Auto");
    }

    private void EvaluateTransitions(string triggerType) {
        if (this.currentStage == null) return;
        var transitions = this.currentStage.Transitions.Where(t => t.TriggerType.Equals(triggerType, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var transition in transitions) {
            if (string.IsNullOrWhiteSpace(transition.ConditionExpression) || this.conditionEvaluator.Evaluate(transition.ConditionExpression, this.context)) {
                this.SetStage(transition.TargetStageId);
                return;
            }
        }
    }

    private bool IsModuleTriggeredByEvent(GameModuleConfig module, GameEvent gameEvent) {
        if (gameEvent is ChatGameEvent chatEvent && module.ModuleType.Equals("ChatListener", StringComparison.OrdinalIgnoreCase)) {
            if (module.Parameters.TryGetValue("Command", out var cmd)) return chatEvent.Message.Trim().StartsWith(cmd, StringComparison.OrdinalIgnoreCase);
            return true;
        }

        if (gameEvent is DiceRollGameEvent && module.ModuleType.Equals("DiceListener", StringComparison.OrdinalIgnoreCase)) return true;

        if (gameEvent is EmoteGameEvent emoteEvent && module.ModuleType.Equals("EmoteListener", StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }
}