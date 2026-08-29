using System;
using HarmonyLib;
using Skald.Configuration;
using Skald.Logic;
using UnityEngine;

namespace Skald.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    public static class PlayerDeathPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Player __instance)
        {
            if (__instance == null) return;

            try
            {
                if (!ModConfig.EnableDeathAnnouncements.Value)
                    return;

                string victimName = __instance.GetPlayerName();
                if (string.IsNullOrWhiteSpace(victimName))
                {
                    victimName = "A Viking";
                }

                // Get last hit data
                HitData? lastHit = Traverse.Create(__instance).Field("m_lastHit").GetValue<HitData>();

                // Analyze death category and killer
                var (category, killerName) = DeathAnalyzer.AnalyzeDeath(__instance, lastHit);

                if (category == DeathCategory.Pvp && !ModConfig.EnablePvpAnnouncements.Value)
                    return;

                // Determine biome
                Vector3 playerPos = __instance.transform.position;
                string biomeName = "Wilds";
                if (WorldGenerator.instance != null)
                {
                    biomeName = WorldGenerator.instance.GetBiome(playerPos.x, playerPos.z).ToString();
                }

                // Format message
                string announcement = DeathAnalyzer.FormatDeathMessage(category, victimName, killerName, biomeName);

                // Get SteamID / Host info if peer exists
                string victimSteamId = string.Empty;
                if (ZNet.instance != null)
                {
                    ZDO? zdo = __instance.m_nview?.GetZDO();
                    if (zdo != null)
                    {
                        foreach (var peer in ZNet.instance.GetPeers())
                        {
                            if (peer.m_characterID == zdo.m_uid)
                            {
                                victimSteamId = peer.m_rpc?.GetSocket()?.GetHostName() ?? string.Empty;
                                break;
                            }
                        }
                    }
                }

                // Record in history for WebPortal
                var record = new DeathEventRecord
                {
                    VictimName = victimName,
                    VictimSteamId = victimSteamId,
                    KillerName = killerName,
                    Category = category,
                    Biome = biomeName,
                    Position = playerPos,
                    FormattedMessage = announcement,
                    Timestamp = DateTime.UtcNow
                };
                ChronicleRegistry.RecordDeath(record);

                if (ModConfig.LogToConsole.Value)
                {
                    SkaldPlugin.Log.LogInfo($"[Skald] 💀 {announcement} (Pos: {playerPos})");
                }

                // Broadcast in-game shout to all players via native routed RPC
                if (ZRoutedRpc.instance != null)
                {
                    ZRoutedRpc.instance.InvokeRoutedRPC(
                        ZRoutedRpc.Everybody,
                        "ChatMessage",
                        playerPos,
                        2, // 2 = Global Shout / Announcement
                        "Skald",
                        announcement
                    );
                }
            }
            catch (Exception ex)
            {
                SkaldPlugin.Log.LogError($"[Skald] Error processing player death announcement: {ex}");
            }
        }
    }
}
