namespace RolePlayer.API.Interop.Services;

using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using RolePlayer.API.Interop.Contracts;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

public unsafe class NativeExecutionService : INativeExecutionService, IDisposable {
    private RaptureMacroModule.Macro* executionMacro;

    public NativeExecutionService() {
        this.executionMacro = (RaptureMacroModule.Macro*)NativeMemory.AllocZeroed((nuint)sizeof(RaptureMacroModule.Macro));

        this.executionMacro->Name.Ctor();
        for (int i = 0; i < 15; i++) this.executionMacro->Lines[i].Ctor();
    }

    public void Execute(string commandOrMacroContent) {
        if (string.IsNullOrWhiteSpace(commandOrMacroContent)) return;

        var shellModule = RaptureShellModule.Instance();
        if (shellModule == null) return;

        var lines = commandOrMacroContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        this.executionMacro->Name.SetString(string.Empty);

        for (int i = 0; i < 15; i++) {
            if (i < lines.Length) {
                var parsedBytes = this.ParseMacroLine(lines[i]);
                fixed (byte* ptr = parsedBytes) {
                    this.executionMacro->Lines[i].SetString(ptr);
                }
            }
            else {
                this.executionMacro->Lines[i].SetString(string.Empty);
            }
        }

        shellModule->ExecuteMacro(this.executionMacro);
    }

    private byte[] ParseMacroLine(string line) {
        var regex = new Regex(@"<at:(\d+):(\d+):([^>]+)>");
        var matches = regex.Matches(line);

        using var ms = new MemoryStream();
        int lastIndex = 0;

        foreach (Match match in matches) {
            if (match.Index > lastIndex) {
                var textBytes = Encoding.UTF8.GetBytes(line.Substring(lastIndex, match.Index - lastIndex));
                ms.Write(textBytes, 0, textBytes.Length);
            }

            if (uint.TryParse(match.Groups[1].Value, out uint group) && uint.TryParse(match.Groups[2].Value, out uint key)) {
                var payload = new AutoTranslatePayload(group, key);
                var payloadBytes = payload.Encode();
                ms.Write(payloadBytes, 0, payloadBytes.Length);
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < line.Length) {
            var textBytes = Encoding.UTF8.GetBytes(line.Substring(lastIndex));
            ms.Write(textBytes, 0, textBytes.Length);
        }

        ms.WriteByte(0);
        return ms.ToArray();
    }

    public void Dispose() {
        if (this.executionMacro == null) return;

        this.executionMacro->Name.Dtor();
        for (int i = 0; i < 15; i++) this.executionMacro->Lines[i].Dtor();

        NativeMemory.Free(this.executionMacro);
        this.executionMacro = null;
    }
}