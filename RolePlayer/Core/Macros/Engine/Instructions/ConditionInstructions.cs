namespace RolePlayer.Core.Macros.Engine.Instructions;

using RolePlayer.Core.Expressions.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Models;

public class IfInstruction : IMacroInstruction {
    private string conditionExpression;
    private IExpressionEvaluator evaluator;

    public IfInstruction(string conditionExpression, IExpressionEvaluator evaluator) {
        this.conditionExpression = conditionExpression;
        this.evaluator = evaluator;
    }

    public void Execute(MacroExecutionContext context) {
        bool result = this.evaluator.Evaluate(this.conditionExpression, varName => {
            if (context.Variables.TryGetValue(varName, out var val)) return val;
            return null;
        });

        context.LastConditionResult = result;

        if (!result) {
            var frame = context.CallStack.Peek();
            if (frame.ProgramCounter < frame.Instructions.Count) {
                frame.ProgramCounter++;
            }
        }
    }
}

public class ElseInstruction : IMacroInstruction {
    public void Execute(MacroExecutionContext context) {
        if (!context.LastConditionResult.HasValue) return;

        bool previousResult = context.LastConditionResult.Value;
        context.LastConditionResult = null;

        if (previousResult) {
            var frame = context.CallStack.Peek();
            if (frame.ProgramCounter < frame.Instructions.Count) {
                frame.ProgramCounter++;
            }
        }
    }
}