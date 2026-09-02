namespace RolePlayer.API.Penumbra.Providers;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.IO;

public class PenumbraIpcProvider : IModStateProvider {
    private ICallGateSubscriber<string, string> resolvePlayerPath;

    public PenumbraIpcProvider(IDalamudPluginInterface pluginInterface) {
        this.resolvePlayerPath = pluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolvePlayerPath");
    }

    public string GetModNameModifyingEmote(uint emoteId) {
        var gamePath = this.GetEmoteGamePath(emoteId);
        if (string.IsNullOrEmpty(gamePath)) {
            return string.Empty;
        }

        try {
            // Appel IPC à Penumbra : "Résous-moi ce chemin pour le joueur actuel"
            var resolvedPath = this.resolvePlayerPath.InvokeFunc(gamePath);

            // Si le chemin retourné est différent du chemin du jeu, c'est qu'un mod est actif
            if (resolvedPath != null && !resolvedPath.Equals(gamePath, StringComparison.OrdinalIgnoreCase)) {
                return this.ExtractModNameFromPath(resolvedPath);
            }
        }
        catch (Exception) {
            // L'IPC échoue si Penumbra n'est pas lancé, on ignore silencieusement
            return string.Empty;
        }

        return string.Empty;
    }

    private string GetEmoteGamePath(uint emoteId) {
        // TODO: Implémenter la résolution via Lumina (ActionTimeline) dans la feature GameData.
        // Pour l'instant, on simule un format de chemin d'animation FFXIV standard.
        return $"chara/action/emote/e{emoteId:D4}.pap";
    }

    private string ExtractModNameFromPath(string resolvedPath) {
        try {
            // Penumbra stocke ses mods dans des dossiers. Le chemin ressemble à :
            // C:\...\Penumbra\Mods\Nom Du Mod\chara\action\emote\e0001.pap
            // On peut extraire le nom du dossier précédant la structure native du jeu ("chara").
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