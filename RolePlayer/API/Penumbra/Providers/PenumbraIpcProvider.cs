namespace RolePlayer.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.IO;

public class PenumbraIpcProvider : IModStateProvider, IDisposable {
    private ICallGateSubscriber<string, string> resolvePlayerPath;
    private IEmotePathProvider emotePathProvider;

    private ICallGateSubscriber<Action>? initialized;
    private ICallGateSubscriber<Action>? disposed;

    // Déclaration explicite du délégué pour garantir la stabilité de l'abonnement/désabonnement
    private Action onPenumbraStateChanged;

    public event Action? ModStateChanged;

    public PenumbraIpcProvider(IDalamudPluginInterface pluginInterface, IEmotePathProvider emotePathProvider) {
        this.resolvePlayerPath = pluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath");
        this.emotePathProvider = emotePathProvider;

        this.onPenumbraStateChanged = () => this.ModStateChanged?.Invoke();

        try {
            // Nous nous abonnons uniquement aux événements globaux sans paramètres propriétaires
            this.initialized = pluginInterface.GetIpcSubscriber<Action>("Penumbra.Initialized");
            if (this.initialized != null) {
                this.initialized.Subscribe(this.onPenumbraStateChanged);
            }

            this.disposed = pluginInterface.GetIpcSubscriber<Action>("Penumbra.Disposed");
            if (this.disposed != null) {
                this.disposed.Subscribe(this.onPenumbraStateChanged);
            }
        }
        catch {
            // Ignorer silencieusement si l'IPC de Penumbra n'est pas disponible
        }
    }

    public string GetModNameModifyingEmote(uint emoteId) {
        var gamePaths = this.emotePathProvider.GetEmoteGamePaths(emoteId);

        foreach (var gamePath in gamePaths) {
            if (string.IsNullOrEmpty(gamePath)) {
                continue;
            }

            try {
                var resolvedPath = this.resolvePlayerPath.InvokeFunc(gamePath);

                if (resolvedPath != null && !resolvedPath.Equals(gamePath, StringComparison.OrdinalIgnoreCase)) {
                    return this.ExtractModNameFromPath(resolvedPath);
                }
            }
            catch (Exception) {
                // Échoue silencieusement si Penumbra n'est pas actif
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
        try {
            if (this.initialized != null) {
                this.initialized.Unsubscribe(this.onPenumbraStateChanged);
            }

            if (this.disposed != null) {
                this.disposed.Unsubscribe(this.onPenumbraStateChanged);
            }
        }
        catch {
            // Évite de faire crasher le déchargement du plugin si l'IPC est corrompu
        }
    }
}