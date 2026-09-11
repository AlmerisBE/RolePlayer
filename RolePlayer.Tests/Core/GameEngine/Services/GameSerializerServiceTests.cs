namespace RolePlayer.Tests.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;
using Xunit;

public class GameSerializerServiceTests {
    [Fact]
    public void SerializeAndDeserialize_MaintainsDataIntegrity() {
        var service = new GameSerializerService();
        var originalGame = new GameDefinition {
            Name = "Test Game",
            InitialVariables = new Dictionary<string, object> { { "MaxRoll", 999 } },
            Stages = new List<GameStage> {
                new GameStage { Id = "stage1", Name = "Registration" }
            }
        };

        var json = service.Serialize(originalGame);
        var restoredGame = service.Deserialize(json);

        Assert.NotNull(restoredGame);
        Assert.Equal("Test Game", restoredGame.Name);
        Assert.Single(restoredGame.Stages);
        Assert.Equal("stage1", restoredGame.Stages[0].Id);
    }

    [Fact]
    public void Base64ExportAndImport_SuccessfullyTransfersGameDefinition() {
        var service = new GameSerializerService();
        var originalGame = new GameDefinition { Name = "Base64 Game" };

        var base64String = service.ToBase64Export(originalGame);
        var restoredGame = service.FromBase64Import(base64String);

        Assert.NotNull(restoredGame);
        Assert.Equal("Base64 Game", restoredGame.Name);
    }

    [Fact]
    public void Deserialize_WithInvalidJson_ReturnsNull() {
        var service = new GameSerializerService();
        var result = service.Deserialize("invalid json format");

        Assert.Null(result);
    }
}