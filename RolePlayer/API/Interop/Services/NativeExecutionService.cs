namespace RolePlayer.API.Interop.Services;

using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using RolePlayer.API.Interop.Contracts;
using System;
using System.Runtime.InteropServices;

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

        var lines = commandOrMacroContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        this.executionMacro->Name.SetString(string.Empty);

        for (int i = 0; i < 15; i++) {
            if (i < lines.Length) this.executionMacro->Lines[i].SetString(lines[i].Trim());
            else this.executionMacro->Lines[i].SetString(string.Empty);
        }

        shellModule->ExecuteMacro(this.executionMacro);
    }

    public void Dispose() {
        if (this.executionMacro == null) return;

        this.executionMacro->Name.Dtor();
        for (int i = 0; i < 15; i++) this.executionMacro->Lines[i].Dtor();

        NativeMemory.Free(this.executionMacro);
        this.executionMacro = null;
    }
}