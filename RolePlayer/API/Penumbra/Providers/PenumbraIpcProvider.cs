namespace RolePlayer.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.IO;

public class PenumbraIpcProvider : IModStateProvider {
    private ICallGateSubscriber<string, string> resolvePlayerPath;
    private IEmotePathProvider emotePathProvider;

    public PenumbraIpcProvider(IDalamudPluginInterface pluginInterface, IEmotePathProvider emotePathProvider) {
        this.resolvePlayerPath = pluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath");
        this.emotePathProvider = emotePathProvider;
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
                // IPC échoue silencieusement si Penumbra n'est pas lancé
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
}