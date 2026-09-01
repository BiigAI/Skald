using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skald.Logic
{
    public static class BroadcastHelper
    {
        public static void BroadcastDeathMessage(string announcement, Vector3 position)
        {
            if (string.IsNullOrWhiteSpace(announcement)) return;

            string cleanMsg = announcement.Trim();

            // 1. Center Screen Banner Announcement (MessageHud.MessageType.Center = 2)
            try
            {
                if (ZRoutedRpc.instance != null)
                {
                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ShowMessage", new object[]
                    {
                        (int)MessageHud.MessageType.Center,
                        cleanMsg
                    });
                }
            }
            catch (Exception ex)
            {
                SkaldPlugin.Log?.LogWarning($"[BroadcastHelper] ZRoutedRpc ShowMessage broadcast failed: {ex.Message}");
            }

            // 2. Chat Box Broadcast across all known Valheim RPC overloads
            try
            {
                if (ZRoutedRpc.instance != null)
                {
                    // 4-parameter standard: (pos, type, name, text)
                    try
                    {
                        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                        {
                            position,
                            (int)Talker.Type.Shout,
                            "Skald",
                            cleanMsg
                        });
                    }
                    catch { }

                    // 5-parameter variant: (pos, type, name, text, userinfo)
                    try
                    {
                        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                        {
                            position,
                            (int)Talker.Type.Shout,
                            "Skald",
                            cleanMsg,
                            string.Empty
                        });
                    }
                    catch { }

                    // Modern UserInfo variant
                    try
                    {
                        var userInfo = new UserInfo { Name = "Skald" };
                        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                        {
                            position,
                            (int)Talker.Type.Shout,
                            userInfo,
                            cleanMsg
                        });
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                SkaldPlugin.Log?.LogWarning($"[BroadcastHelper] ZRoutedRpc ChatMessage broadcast failed: {ex.Message}");
            }

            // 3. Direct Peer RPC invocation for all active connections
            try
            {
                if (ZNet.instance != null)
                {
                    var peers = ZNet.instance.GetPeers();
                    if (peers != null)
                    {
                        foreach (var peer in peers)
                        {
                            if (peer?.m_rpc != null)
                            {
                                try
                                {
                                    peer.m_rpc.Invoke("ShowMessage", new object[]
                                    {
                                        (int)MessageHud.MessageType.Center,
                                        cleanMsg
                                    });
                                }
                                catch { }

                                try
                                {
                                    peer.m_rpc.Invoke("ChatMessage", new object[]
                                    {
                                        position,
                                        (int)Talker.Type.Shout,
                                        "Skald",
                                        cleanMsg
                                    });
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SkaldPlugin.Log?.LogWarning($"[BroadcastHelper] Direct peer broadcast failed: {ex.Message}");
            }

            // 4. Local client UI fallback (for singleplayer, local host, or local client)
            try
            {
                if (MessageHud.instance != null)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, cleanMsg);
                }

                if (Chat.instance != null)
                {
                    Chat.instance.AddString(cleanMsg);
                }
            }
            catch (Exception ex)
            {
                SkaldPlugin.Log?.LogWarning($"[BroadcastHelper] Local client UI display failed: {ex.Message}");
            }
        }
    }
}
