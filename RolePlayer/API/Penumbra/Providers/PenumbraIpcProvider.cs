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
        var gamePath = this.emotePathProvider.GetEmoteGamePath(emoteId);
        if (string.IsNullOrEmpty(gamePath)) {
            return string.Empty;
        }

        try {
            var resolvedPath = this.resolvePlayerPath.InvokeFunc(gamePath);

            if (resolvedPath != null && !resolvedPath.Equals(gamePath, StringComparison.OrdinalIgnoreCase)) {
                return this.ExtractModNameFromPath(resolvedPath);
            }
        }
        catch (Exception) {
            return string.Empty;
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