namespace RolePlayer.Core.Macros.Engine.Services;

using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using RolePlayer.Core.Macros.Engine.Models;
using RolePlayer.Core.Macros.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class MacroCompiler : IMacroCompiler {
    private IEnumerable<IInstructionParser> parsers;
    private INativeExecutionService nativeExecution;

    public MacroCompiler(IEnumerable<IInstructionParser> parsers, INativeExecutionService nativeExecution) {
        this.parsers = parsers;
        this.nativeExecution = nativeExecution;
    }

    public bool TryCompile(RoleplayMacro macro, out MacroCallFrame? frame, out string errorMessage) {
        frame = null;
        errorMessage = string.Empty;

        if (macro == null || string.IsNullOrWhiteSpace(macro.Content)) {
            errorMessage = "Macro is empty.";
            return false;
        }

        var instructions = new List<IMacroInstruction>();
        var labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lines = macro.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            IMacroInstruction? instruction = null;
            string? parserError = null;
            bool handled = false;

            foreach (var parser in this.parsers) {
                if (parser.TryParse(line, out instruction, out parserError)) {
                    handled = true;
                    break;
                }
            }

            if (handled && !string.IsNullOrEmpty(parserError)) {
                errorMessage = $"Line {i + 1}: {parserError}";
                return false;
            }

            if (!handled) {
                var waitMatch = Regex.Match(line, @"<wait\.\s*([0-9\.]+)\s*>", RegexOptions.IgnoreCase);

                if (waitMatch.Success) {
                    string cleanLine = line.Replace(waitMatch.Value, string.Empty).Trim();

                    if (!string.IsNullOrWhiteSpace(cleanLine)) instructions.Add(new NativeCommandInstruction(cleanLine, this.nativeExecution));
                    if (float.TryParse(waitMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float delay)) instructions.Add(new WaitInstruction(delay));
                }
                else {
                    instructions.Add(new NativeCommandInstruction(line, this.nativeExecution));
                }
            }
            else if (instruction != null) {
                if (instruction is LabelInstruction labelInst) {
                    if (labels.ContainsKey(labelInst.LabelName)) {
                        errorMessage = $"Line {i + 1}: Duplicate label found '{labelInst.LabelName}'";
                        return false;
                    }
                    labels[labelInst.LabelName] = instructions.Count;
                }
                instructions.Add(instruction);
            }
        }

        frame = new MacroCallFrame {
            MacroId = macro.Id,
            Instructions = instructions,
            Labels = labels
        };

        return true;
    }
}