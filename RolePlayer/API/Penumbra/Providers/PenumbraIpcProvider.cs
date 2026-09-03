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
using System.Collections.Generic;
using System.IO;

public class PenumbraIpcProvider : IModStateProvider, IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IEmotePathProvider emotePathProvider;
    private ILoggerService logger;

    private ICallGateSubscriber<string, string> resolvePlayerPathSubscriber;
    private ApiVersion apiVersionSubscriber;
    private GetModList getModListSubscriber;
    private GetModDirectory getModDirectorySubscriber;

    private Action onPenumbraLifecycleChanged;
    private Action<ModSettingChange, Guid, string, bool> onModSettingChanged;

    private IDisposable? initializedSubscriber;
    private IDisposable? disposedSubscriber;
    private IDisposable? modSettingChangedSubscriber;

    private IReadOnlyDictionary<string, string> modNamesCache = new Dictionary<string, string>();
    private string penumbraRootPath = string.Empty;

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
        this.getModListSubscriber = new GetModList(pluginInterface);
        this.getModDirectorySubscriber = new GetModDirectory(pluginInterface);

        this.onPenumbraLifecycleChanged = () => {
            this.logger.Debug("[PenumbraIpcProvider] Penumbra lifecycle event detected. Updating cache and triggering UI refresh.");
            this.UpdateModCache();
            this.ModStateChanged?.Invoke();
        };

        this.onModSettingChanged = (type, collectionId, modDirectory, inherited) => {
            this.logger.Debug($"[PenumbraIpcProvider] ModSettingChanged event detected (Type: {type}, Mod: {modDirectory}). Updating cache and triggering UI refresh.");
            this.UpdateModCache();
            this.ModStateChanged?.Invoke();
        };

        try {
            this.logger.Debug("[PenumbraIpcProvider] Initializing Penumbra API static event subscribers...");

            this.initializedSubscriber = Initialized.Subscriber(this.pluginInterface, this.onPenumbraLifecycleChanged);
            this.disposedSubscriber = Disposed.Subscriber(this.pluginInterface, this.onPenumbraLifecycleChanged);
            this.modSettingChangedSubscriber = ModSettingChanged.Subscriber(this.pluginInterface, this.onModSettingChanged);
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[PenumbraIpcProvider] Failed to subscribe to Penumbra static IPC events.");
        }

        this.UpdateModCache();
    }

    private void UpdateModCache() {
        try {
            if (!this.IsEnabled()) {
                this.modNamesCache = new Dictionary<string, string>();
                this.penumbraRootPath = string.Empty;
                return;
            }

            this.penumbraRootPath = this.getModDirectorySubscriber.Invoke();
            var mods = this.getModListSubscriber.Invoke();

            // Atomic allocation for thread-safety during background reading
            var newCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in mods) {
                newCache[kvp.Key] = kvp.Value;
            }

            this.modNamesCache = newCache;
        }
        catch (Exception ex) {
            this.logger.Verbose($"[PenumbraIpcProvider] Failed to refresh Mod Directory or Mod List: {ex.Message}");
            this.modNamesCache = new Dictionary<string, string>();
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
            // Strategic isolation of the mod directory using the exact Penumbra Root Directory
            if (!string.IsNullOrEmpty(this.penumbraRootPath) && resolvedPath.StartsWith(this.penumbraRootPath, StringComparison.OrdinalIgnoreCase)) {
                var relativePath = resolvedPath.Substring(this.penumbraRootPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var parts = relativePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0) {
                    var modDirectory = parts[0];
                    if (this.modNamesCache.TryGetValue(modDirectory, out var realModName)) {
                        return realModName;
                    }

                    return modDirectory;
                }
            }

            // Fallback for edge cases outside the standard root path
            var fallbackParts = resolvedPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            int pivotIndex = -1;

            for (int i = 0; i < fallbackParts.Length; i++) {
                if (fallbackParts[i].Equals("chara", StringComparison.OrdinalIgnoreCase) || fallbackParts[i].Equals("animation", StringComparison.OrdinalIgnoreCase)) {
                    pivotIndex = i;
                    break;
                }
            }

            if (pivotIndex > 0) {
                var modDirectory = fallbackParts[pivotIndex - 1];
                if (this.modNamesCache.TryGetValue(modDirectory, out var realModName)) {
                    return realModName;
                }

                return modDirectory;
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