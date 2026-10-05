namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Linq;

public class RegisterPlayerHandler : IGameActionHandler {
    public string ActionType => "RegisterPlayer";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (context.CurrentEvent == null || string.IsNullOrWhiteSpace(context.CurrentEvent.Sender)) return;

        string sender = context.CurrentEvent.Sender;
        if (!context.Participants.Contains(sender, StringComparer.OrdinalIgnoreCase)) {
            context.Participants.Add(sender);
        }
    }
}