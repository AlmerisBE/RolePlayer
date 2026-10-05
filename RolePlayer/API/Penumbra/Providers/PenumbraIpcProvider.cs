namespace RolePlayer.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using global::Penumbra.Api.Enums;
using global::Penumbra.Api.IpcSubscribers;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

public class PenumbraIpcProvider : IEmoteModState, IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IEmotePathProvider emotePathProvider;
    private ILoggerService logger;
    private IFramework framework;
    private IObjectTable objectTable;

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

    private List<uint> trackedEmotes = new();
    private Dictionary<uint, string> trackedModStates = new();
    private int frameCounter = 0;
    private int pollIndex = 0;

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
        this.getModDirectorySubscriber = new GetModDirectory(pluginInterface);

        this.onPenumbraLifecycleChanged = () => {
            this.logger.Debug("[PenumbraIpcProvider] Penumbra lifecycle event detected. Updating cache and triggering UI refresh.");
            this.UpdateModCache();
            this.TriggerRefresh();
        };

        this.onModSettingChanged = (type, collectionId, modDirectory, inherited) => {
            this.logger.Debug($"[PenumbraIpcProvider] ModSettingChanged event detected (Type: {type}, Mod: {modDirectory}). Updating cache and triggering UI refresh.");
            this.UpdateModCache();
            this.TriggerRefresh();
        };

        try {
            this.initializedSubscriber = Initialized.Subscriber(this.pluginInterface, this.onPenumbraLifecycleChanged);
            this.disposedSubscriber = Disposed.Subscriber(this.pluginInterface, this.onPenumbraLifecycleChanged);
            this.modSettingChangedSubscriber = ModSettingChanged.Subscriber(this.pluginInterface, this.onModSettingChanged);
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[PenumbraIpcProvider] Failed to subscribe to Penumbra static IPC events.");
        }

        this.UpdateModCache();
        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void TriggerRefresh() {
        this.trackedEmotes.Clear();
        this.trackedModStates.Clear();
        this.ModStateChanged?.Invoke();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (this.trackedEmotes.Count == 0 || this.objectTable.LocalPlayer == null) return;

        this.frameCounter++;
        if (this.frameCounter < 30) return;
        this.frameCounter = 0;

        if (this.pollIndex >= this.trackedEmotes.Count) this.pollIndex = 0;

        uint emoteId = this.trackedEmotes[this.pollIndex];
        string cachedModName = this.trackedModStates[emoteId];
        string currentModName = this.ResolveEffectiveModName(emoteId);

        if (!string.Equals(cachedModName, currentModName, StringComparison.OrdinalIgnoreCase)) {
            this.logger.Debug($"[PenumbraIpcProvider] Smart polling detected effective mod change for emote {emoteId} (e.g. Hierarchy/Inheritance). Triggering refresh.");
            this.TriggerRefresh();
            this.pollIndex = 0;
            return;
        }

        this.pollIndex++;
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
        string modName = this.ResolveEffectiveModName(emoteId);

        if (!this.trackedModStates.ContainsKey(emoteId)) {
            this.trackedEmotes.Add(emoteId);
        }

        this.trackedModStates[emoteId] = modName;
        return modName;
    }

    private string ResolveEffectiveModName(uint emoteId) {
        if (!this.IsEnabled()) return string.Empty;

        var gamePaths = this.emotePathProvider.GetEmoteGamePaths(emoteId);

        foreach (var gamePath in gamePaths) {
            if (string.IsNullOrEmpty(gamePath)) continue;

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
            if (!string.IsNullOrEmpty(this.penumbraRootPath) && resolvedPath.StartsWith(this.penumbraRootPath, StringComparison.OrdinalIgnoreCase)) {
                var relativePath = resolvedPath.Substring(this.penumbraRootPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var parts = relativePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0) {
                    var modDirectory = parts[0];
                    if (this.modNamesCache.TryGetValue(modDirectory, out var realModName)) return realModName;
                    return modDirectory;
                }
            }

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
                if (this.modNamesCache.TryGetValue(modDirectory, out var realModName)) return realModName;
                return modDirectory;
            }

            return "Modded Emote";
        }
        catch {
            return "Modded Emote";
        }
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;

        try {
            if (this.initializedSubscriber != null) this.initializedSubscriber.Dispose();
            if (this.disposedSubscriber != null) this.disposedSubscriber.Dispose();
            if (this.modSettingChangedSubscriber != null) this.modSettingChangedSubscriber.Dispose();
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[PenumbraIpcProvider] Error during IPC unsubscription.");
        }
    }
}