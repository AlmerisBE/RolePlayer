namespace RolePlayer.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
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
    private IFramework framework;
    private IObjectTable objectTable;

    private ICallGateSubscriber<string, string> resolvePlayerPathSubscriber;
    private ApiVersion apiVersionSubscriber;
    private GetModList getModListSubscriber;
    private GetCollectionForObject getCollectionForObjectSubscriber;

    private ICallGateSubscriber<Action>? initializedSubscriber;
    private ICallGateSubscriber<Action>? disposedSubscriber;
    private Action onPenumbraLifecycleChanged;

    private DateTime lastCheckTime = DateTime.MinValue;
    private int lastModCount = -1;
    private Guid lastCollectionId = Guid.Empty;
    private string lastPlayerName = string.Empty;

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
        this.framework = framework;
        this.objectTable = objectTable;

        this.resolvePlayerPathSubscriber = pluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath");
        this.apiVersionSubscriber = new ApiVersion(pluginInterface);
        this.getModListSubscriber = new GetModList(pluginInterface);
        this.getCollectionForObjectSubscriber = new GetCollectionForObject(pluginInterface);

        // Instance de l'action unique pour un désabonnement garanti sans fuite mémoire
        this.onPenumbraLifecycleChanged = this.OnPenumbraLifecycleChanged;

        try {
            this.logger.Debug("[PenumbraIpcProvider] Initializing native Penumbra lifecycle subscribers...");

            this.initializedSubscriber = pluginInterface.GetIpcSubscriber<Action>("Penumbra.Initialized");
            if (this.initializedSubscriber != null) {
                this.initializedSubscriber.Subscribe(this.onPenumbraLifecycleChanged);
            }

            this.disposedSubscriber = pluginInterface.GetIpcSubscriber<Action>("Penumbra.Disposed");
            if (this.disposedSubscriber != null) {
                this.disposedSubscriber.Subscribe(this.onPenumbraLifecycleChanged);
            }
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[PenumbraIpcProvider] Failed to subscribe to Penumbra lifecycle IPC events.");
        }

        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnPenumbraLifecycleChanged() {
        this.logger.Debug("[PenumbraIpcProvider] Penumbra lifecycle event detected. Forcing background cache invalidation.");
        this.lastCheckTime = DateTime.MinValue;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if ((DateTime.Now - this.lastCheckTime).TotalSeconds < 2.0) {
            return;
        }

        this.lastCheckTime = DateTime.Now;

        if (!this.IsEnabled()) {
            return;
        }

        try {
            var currentPlayerName = this.objectTable.LocalPlayer?.Name.TextValue ?? string.Empty;
            var currentModList = this.getModListSubscriber.Invoke();
            var currentModCount = currentModList?.Count ?? 0;

            var collectionResult = this.getCollectionForObjectSubscriber.Invoke(0);
            var currentCollectionId = collectionResult.EffectiveCollection.Id;

            bool hasChanged = currentPlayerName != this.lastPlayerName
                           || currentModCount != this.lastModCount
                           || currentCollectionId != this.lastCollectionId;

            if (hasChanged) {
                this.logger.Debug($"[PenumbraIpcProvider] Mod state change detected (Player: {currentPlayerName}, Mods: {currentModCount}, Collection: {currentCollectionId}). Firing UI refresh event.");

                this.lastPlayerName = currentPlayerName;
                this.lastModCount = currentModCount;
                this.lastCollectionId = currentCollectionId;

                this.ModStateChanged?.Invoke();
            }
        }
        catch (Exception ex) {
            this.logger.Verbose($"[PenumbraIpcProvider] Background sync check encountered an error: {ex.Message}");
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
        this.logger.Debug("[PenumbraIpcProvider] Disposing IPC polling processes.");
        this.framework.Update -= this.OnFrameworkUpdate;

        try {
            if (this.initializedSubscriber != null) {
                this.initializedSubscriber.Unsubscribe(this.onPenumbraLifecycleChanged);
            }

            if (this.disposedSubscriber != null) {
                this.disposedSubscriber.Unsubscribe(this.onPenumbraLifecycleChanged);
            }
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[PenumbraIpcProvider] Error during IPC unsubscription.");
        }
    }
}