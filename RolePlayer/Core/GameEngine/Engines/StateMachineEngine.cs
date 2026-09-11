namespace RolePlayer.Core.GameEngine.Engines;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachineEngine : IGameEngine {
    private IConditionEvaluatorService conditionEvaluator;
    private IGameActionExecutionService actionExecutionService;

    private GameSessionConfig? config;
    private GameSessionContext context = new();
    private GameStage? currentStage;

    public string EngineType => "StateMachineEngine";
    public bool IsRunning { get; private set; }

    public IReadOnlyList<string> Participants => this.context.Participants.ToList();
    public string CurrentStageName => this.currentStage?.Name ?? "Unknown";

    public bool AllowChatRegistration { get; set; } = false;

    public event Action<string>? BroadcastRequested;
    public event Action? GameFinished;
    public event Action? ParticipantsChanged;
    public event Action? StageChanged;

    public StateMachineEngine(IConditionEvaluatorService conditionEvaluator, IGameActionExecutionService actionExecutionService) {
        this.conditionEvaluator = conditionEvaluator;
        this.actionExecutionService = actionExecutionService;

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

        this.SetStage(this.config.Game.Stages.First().Id);
    }

    public void Stop() {
        if (!this.IsRunning) return;
        this.IsRunning = false;
        this.GameFinished?.Invoke();
    }

    public void AddParticipant(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;

        if (this.context.Participants.Add(name)) {
            this.ParticipantsChanged?.Invoke();
        }
    }

    public void RemoveParticipant(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;

        if (this.context.Participants.Remove(name)) {
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

                // If an action added a player, notify UI
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
        this.StageChanged?.Invoke();

        this.actionExecutionService.ExecuteAll(this.currentStage.OnEnterActions, this.context);

        this.EvaluateTransitions("Auto");
    }

    private void EvaluateTransitions(string triggerType) {
        if (this.currentStage == null) return;

        var transitions = this.currentStage.Transitions
            .Where(t => t.TriggerType.Equals(triggerType, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var transition in transitions) {
            if (string.IsNullOrWhiteSpace(transition.ConditionExpression) ||
                this.conditionEvaluator.Evaluate(transition.ConditionExpression, this.context)) {

                this.SetStage(transition.TargetStageId);
                return; // Ensure we only take one transition at a time
            }
        }
    }

    private bool IsModuleTriggeredByEvent(GameModuleConfig module, GameEvent gameEvent) {
        if (gameEvent is ChatGameEvent chatEvent && module.ModuleType.Equals("ChatListener", StringComparison.OrdinalIgnoreCase)) {
            if (module.Parameters.TryGetValue("Command", out var cmd)) {
                return chatEvent.Message.Trim().StartsWith(cmd, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        if (gameEvent is DiceRollGameEvent && module.ModuleType.Equals("DiceListener", StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        if (gameEvent is EmoteGameEvent emoteEvent && module.ModuleType.Equals("EmoteListener", StringComparison.OrdinalIgnoreCase)) {
            if (module.Parameters.TryGetValue("EmoteId", out var reqEmoteIdStr) && uint.TryParse(reqEmoteIdStr, out uint reqEmoteId)) {
                return emoteEvent.EmoteId == reqEmoteId;
            }
            return true;
        }

        return false;
    }
}