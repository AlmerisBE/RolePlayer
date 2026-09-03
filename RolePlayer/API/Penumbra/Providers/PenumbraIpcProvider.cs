namespace RolePlayer.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using global::Penumbra.Api.IpcSubscribers;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.IO;

public class PenumbraIpcProvider : IModStateProvider, IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IEmotePathProvider emotePathProvider;

    private ApiVersion apiVersionSubscriber;
    private ICallGateSubscriber<string, string> resolvePlayerPathSubscriber;

    private ICallGateSubscriber<Action> initializedSubscriber;
    private ICallGateSubscriber<Action> disposedSubscriber;

    public event Action? ModStateChanged;

    public PenumbraIpcProvider(IDalamudPluginInterface pluginInterface, IEmotePathProvider emotePathProvider) {
        this.pluginInterface = pluginInterface;
        this.emotePathProvider = emotePathProvider;

        this.apiVersionSubscriber = new ApiVersion(pluginInterface);
        this.resolvePlayerPathSubscriber = pluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath");

        this.initializedSubscriber = pluginInterface.GetIpcSubscriber<Action>("Penumbra.Initialized");
        this.initializedSubscriber.Subscribe(this.HandleInitialized);

        this.disposedSubscriber = pluginInterface.GetIpcSubscriber<Action>("Penumbra.Disposed");
        this.disposedSubscriber.Subscribe(this.HandleDisposed);
    }

    private void HandleInitialized() {
        this.ModStateChanged?.Invoke();
    }

    private void HandleDisposed() {
        this.ModStateChanged?.Invoke();
    }

    private bool IsEnabled() {
        try {
            this.apiVersionSubscriber.Invoke();
            return true;
        }
        catch {
            return false;
        }
    }

    public string GetModNameModifyingEmote(uint emoteId) {
        if (!this.IsEnabled()) {
            return string.Empty;
        }

        var gamePaths = this.emotePathProvider.GetEmoteGamePaths(emoteId);

        foreach (var gamePath in gamePaths) {
            if (string.IsNullOrEmpty(gamePath)) {
                continue;
            }

            try {
                var resolvedPath = this.resolvePlayerPathSubscriber.InvokeFunc(gamePath);

                if (resolvedPath != null && !resolvedPath.Equals(gamePath, StringComparison.OrdinalIgnoreCase)) {
                    return this.ExtractModNameFromPath(resolvedPath);
                }
            }
            catch {
                // Silently fail if IPC throws unexpected errors
            }
        }

        return string.Empty;
    }

    private string ExtractModNameFromPath(string resolvedPath) {
        try {
            var parts = resolvedPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var charaIndex = Array.IndexOf(parts, "chara");

            if (charaIndex > 0) {
                return parts[charaIndex - 1];
            }

            return "Modded Emote";
        }
        catch {
            return "Modded Emote";
        }
    }

    public void Dispose() {
        this.initializedSubscriber.Unsubscribe(this.HandleInitialized);
        this.disposedSubscriber.Unsubscribe(this.HandleDisposed);
    }
}