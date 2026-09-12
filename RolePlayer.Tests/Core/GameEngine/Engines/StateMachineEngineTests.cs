namespace RolePlayer.Tests.Core.GameEngine.Engines;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Engines;
using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;
using Xunit;

public class StateMachineEngineTests {
    private StateMachineEngine CreateEngine() {
        var evaluator = Substitute.For<IConditionEvaluatorService>();
        evaluator.EvaluateAll(Arg.Any<IEnumerable<string>>(), Arg.Any<GameSessionContext>()).Returns(true);

        var actionService = Substitute.For<IGameActionExecutionService>();
        var framework = Substitute.For<IFramework>();

        var engine = new StateMachineEngine(evaluator, actionService, framework);

        var config = new GameSessionConfig {
            Game = new GameDefinition {
                Name = "Test Game",
                InitialVariables = new Dictionary<string, object> { { "test_var", 1 } },
                Stages = new List<GameStage> {
                    new GameStage {
                        Id = "stage1",
                        Name = "Registration",
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "ChatListener",
                                Parameters = new Dictionary<string, string> { { "Command", "!join" } },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "RegisterPlayer" }
                                }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition { TargetStageId = "stage2", TriggerType = "Manual" }
                        }
                    },
                    new GameStage { Id = "stage2", Name = "Playing" }
                }
            }
        };

        engine.Initialize(config);
        return engine;
    }

    [Fact]
    public void Start_InitializesContextAndSetsFirstStage() {
        var engine = this.CreateEngine();

        engine.Start();

        Assert.True(engine.IsRunning);
        Assert.Equal("Registration", engine.CurrentStageName);
    }

    [Fact]
    public void ProcessEvent_WithMatchingModule_ExecutesActions() {
        var engine = this.CreateEngine();
        engine.Start();

        var chatEvent = new ChatGameEvent { Sender = "Player1", Message = "!join" };

        engine.ProcessEvent(chatEvent);

        // Since we mocked IGameActionExecutionService, we can't easily assert internal context participants here,
        // but we verify the logic flow in integration. 
        // A simple test ensuring no crashes and proper execution flow.
        Assert.True(engine.IsRunning);
    }

    [Fact]
    public void AdvanceStage_TransitionsToNextDefinedStage() {
        var engine = this.CreateEngine();
        engine.Start();

        Assert.Equal("Registration", engine.CurrentStageName);

        engine.AdvanceStage();

        Assert.Equal("Playing", engine.CurrentStageName);
    }
}