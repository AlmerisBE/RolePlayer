namespace RolePlayer.Core.GameEngine.Services;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Runtime.InteropServices;
using System.Text;

public class ChatBroadcaster : IChatBroadcaster {
    private IChatGui chatGui;
    private ProcessChatBoxDelegate? processChatBox;

    private unsafe delegate void ProcessChatBoxDelegate(UIModule* uiModule, Utf8String* message, IntPtr unused, byte a4);

    public ChatBroadcaster(IChatGui chatGui, ISigScanner sigScanner) {
        this.chatGui = chatGui;

        try {
            // Native Dawntrail signature for ProcessChatBox
            var ptr = sigScanner.ScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9 74 11");
            if (ptr != IntPtr.Zero) {
                this.processChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(ptr);
            }
        }
        catch (Exception) {
            // Silent fallback: if the signature breaks in a future patch, it will default to local print.
        }
    }

    public unsafe void Broadcast(string message, GameChatChannel channel) {
        if (string.IsNullOrWhiteSpace(message)) return;

        string channelCmd = channel switch {
            GameChatChannel.Say => "/s",
            GameChatChannel.Yell => "/y",
            GameChatChannel.Shout => "/sh",
            GameChatChannel.Party => "/p",
            GameChatChannel.Alliance => "/a",
            GameChatChannel.FreeCompany => "/fc",
            _ => "/e"
        };

        string command = $"{channelCmd} {message}";

        if (this.processChatBox != null) {
            try {
                var uiModule = UIModule.Instance();
                if (uiModule != null) {
                    byte[] bytes = Encoding.UTF8.GetBytes(command + "\0");
                    fixed (byte* ptr = bytes) {
                        var utf8String = new Utf8String();
                        utf8String.SetString(ptr);

                        this.processChatBox(uiModule, &utf8String, IntPtr.Zero, 0);

                        utf8String.Dtor(); // Properly clear allocated string memory
                    }
                    return;
                }
            }
            catch (Exception) {
                // Ignore exception and execute fallback
            }
        }

        // Safe Fallback
        this.chatGui.Print($"[Bot] {command}");
    }
}