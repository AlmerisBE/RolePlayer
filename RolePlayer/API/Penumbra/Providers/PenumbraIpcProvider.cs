namespace RolePlayer.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using global::Penumbra.Api.Enums;
using global::Penumbra.Api.IpcSubscribers;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.IO;

public class PenumbraIpcProvider : IModStateProvider, IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IEmotePathProvider emotePathProvider;
    private ILoggerService logger;

    private ICallGateSubscriber<string, string> resolvePlayerPathSubscriber;
    private ApiVersion apiVersionSubscriber;

    private Action onPenumbraLifecycleChanged;
    private Action<ModSettingChange, Guid, string, bool> onModSettingChanged;

    // Utilisation de IDisposable pour encapsuler l'EventSubscriber retourné par l'API Penumbra
    private IDisposable? initializedSubscriber;
    private IDisposable? disposedSubscriber;
    private IDisposable? modSettingChangedSubscriber;

    public event Action? ModStateChanged;

    public PenumbraIpcProvider(
        IDalamudPluginInterface pluginInterface,
        IEmotePathProvider emotePathProvider,
        ILoggerService logger,
        IFramework framework,
        IObjectTable objectTable) {

        this.pluginInterface = pluginInterface;
        this.emotePathProvider = emotePathProvider;
        this.logger = logger;

        this.resolvePlayerPathSubscriber = pluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath");
        this.apiVersionSubscriber = new ApiVersion(pluginInterface);

        this.onPenumbraLifecycleChanged = () => {
            this.logger.Debug("[PenumbraIpcProvider] Penumbra lifecycle event detected. Triggering UI refresh.");
            this.ModStateChanged?.Invoke();
        };

        this.onModSettingChanged = (type, collectionId, modDirectory, inherited) => {
            this.logger.Debug($"[PenumbraIpcProvider] ModSettingChanged event detected (Type: {type}, Mod: {modDirectory}). Triggering UI refresh.");
            this.ModStateChanged?.Invoke();
        };

        try {
            this.logger.Debug("[PenumbraIpcProvider] Initializing Penumbra API static event subscribers...");

            // L'appel à la méthode Subscriber retourne le jeton de désabonnement
            this.initializedSubscriber = Initialized.Subscriber(this.pluginInterface, this.onPenumbraLifecycleChanged);
            this.disposedSubscriber = Disposed.Subscriber(this.pluginInterface, this.onPenumbraLifecycleChanged);
            this.modSettingChangedSubscriber = ModSettingChanged.Subscriber(this.pluginInterface, this.onModSettingChanged);
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[PenumbraIpcProvider] Failed to subscribe to Penumbra static IPC events.");
        }
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
        this.logger.Debug("[PenumbraIpcProvider] Disposing IPC static subscriptions.");
        try {
            // Un simple Dispose sur le jeton suffit à couper l'écoute IPC
            if (this.initializedSubscriber != null) {
                this.initializedSubscriber.Dispose();
            }

            if (this.disposedSubscriber != null) {
                this.disposedSubscriber.Dispose();
            }

            if (this.modSettingChangedSubscriber != null) {
                this.modSettingChangedSubscriber.Dispose();
            }
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[PenumbraIpcProvider] Error during IPC unsubscription.");
        }
    }
}