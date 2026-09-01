using System;
using System.Collections.Generic;
using System.Linq;
using Skald.Configuration;
using Skald.Logic;
using UnityEngine;

namespace Skald.Patches
{
    public static class PlayerDeathPatch
    {
        private static readonly Dictionary<string, DateTime> _lastDeathAnnounced = new Dictionary<string, DateTime>();
        private static readonly object _announceLock = new object();

        // ═══════════════════════════════════════════════════════════════════════
        // Death detection: poll raw replicated ZDO health data.
        // Player.OnDeath/RPC_OnDeath/ApplyDamage/RPC_Damage/TombStone.Setup all
        // require a live Player/Character component, which a dedicated server
        // never instantiates for remote clients (confirmed empirically: with a
        // real connected peer, Player.GetAllPlayers() stayed 0). So detection
        // goes through ZNetPeer + ZDOMan directly instead.
        // ═══════════════════════════════════════════════════════════════════════
        private static readonly Dictionary<ZDOID, float> _lastKnownHealth = new Dictionary<ZDOID, float>();
        private static int _pollTick = 0;

        public static void PollForDeaths()
        {
            try
            {
                if (!ModConfig.EnableDeathAnnouncements.Value) return;
                if (ZNet.instance == null || ZDOMan.instance == null) return;

                List<ZNetPeer> peers = ZNet.instance.GetPeers();

                _pollTick++;
                bool logDiag = _pollTick % 10 == 0;
                if (logDiag)
                {
                    SkaldPlugin.Log?.LogInfo($"[Skald][Diag] ZNet peers={peers?.Count ?? -1}");
                }

                if (peers == null) return;

                List<(string name, string steamId, Vector3 pos)> newlyDead = new List<(string, string, Vector3)>();

                foreach (ZNetPeer peer in peers)
                {
                    if (peer == null || peer.m_characterID.IsNone()) continue;

                    ZDO zdo = ZDOMan.instance.GetZDO(peer.m_characterID);
                    if (zdo == null) continue;

                    float health = zdo.GetFloat(ZDOVars.s_health, -1f);
                    if (health < 0f) continue; // field not present yet (character still spawning)

                    string name = zdo.GetString(ZDOVars.s_playerName, string.Empty);
                    if (string.IsNullOrWhiteSpace(name)) name = peer.m_playerName;

                    if (logDiag)
                    {
                        SkaldPlugin.Log?.LogInfo($"[Skald][Diag]   '{name}' health={health:0.0}");
                    }

                    ZDOID id = peer.m_characterID;
                    lock (_announceLock)
                    {
                        if (_lastKnownHealth.TryGetValue(id, out float previous) && previous > 0.01f && health <= 0.01f)
                        {
                            newlyDead.Add((name, peer.m_rpc?.GetSocket()?.GetHostName() ?? string.Empty, zdo.GetPosition()));
                        }
                        _lastKnownHealth[id] = health;
                    }
                }

                foreach (var death in newlyDead)
                {
                    SkaldPlugin.Log?.LogInfo($"[Skald][Hook] ZDO health poll detected death for {death.name}.");
                    ProcessDeathByName(death.name, death.steamId, death.pos);
                }
            }
            catch (Exception ex)
            {
                SkaldPlugin.Log?.LogError($"[Skald] Error polling for deaths: {ex}");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Core Death Announcement Processor
        // ═══════════════════════════════════════════════════════════════════════
        public static void ProcessDeathByName(string victimName, string victimSteamId, Vector3 position)
        {
            if (!ModConfig.EnableDeathAnnouncements.Value)
            {
                SkaldPlugin.Log?.LogInfo("[Skald][Hook] ProcessDeathByName skipped - EnableDeathAnnouncements is disabled in config.");
                return;
            }

            ExecuteAnnouncement(victimName, victimSteamId, position);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Nearby-enemy heuristic (there is no HitData available server-side, so
        // the killer is guessed from what's actually standing near the corpse).
        // Swarm/"overwhelmed" detection stays tight (melee range) so it doesn't
        // false-positive in mob-dense biomes; the killer-name guess alone gets a
        // wider radius to still catch ranged kills.
        // ═══════════════════════════════════════════════════════════════════════
        private const float SwarmRadius = 5f;
        private const float KillerSearchRadius = 20f;
        private const int OverwhelmedThreshold = 3;

        private static List<(string name, float dist)> FindNearbyEnemies(Vector3 position, float maxRadius)
        {
            var result = new List<(string, float)>();
            try
            {
                if (ZDOMan.instance == null || ZNetScene.instance == null) return result;

                Vector2i zone = ZoneSystem.GetZone(position);
                var sectorObjects = new List<ZDO>();
                var distantObjects = new List<ZDO>();
                ZDOMan.instance.FindSectorObjects(zone, 1, 0, sectorObjects, distantObjects);

                SkaldPlugin.Log?.LogInfo($"[Skald][Diag] Enemy scan at {position}: {sectorObjects.Count} ZDO(s) in range of sector.");

                foreach (ZDO zdo in sectorObjects)
                {
                    if (zdo == null) continue;

                    float dist = Vector3.Distance(zdo.GetPosition(), position);
                    if (dist > maxRadius) continue;

                    GameObject prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
                    if (prefab == null || prefab.GetComponent<Player>() != null) continue; // never count players

                    Character character = prefab.GetComponent<Character>();
                    if (character == null) continue;

                    // The health field only exists in the ZDO once it differs from full
                    // health (lazy sync) - a full-health attacker legitimately has none.
                    bool hasHealth = zdo.GetFloat(ZDOVars.s_health, out float health);
                    SkaldPlugin.Log?.LogInfo($"[Skald][Diag]   candidate '{prefab.name}' dist={dist:0.0} hasHealthField={hasHealth} health={health:0.0}");
                    if (hasHealth && health <= 0f) continue; // confirmed already dead, not a threat

                    result.Add((prefab.name, dist));
                }
            }
            catch (Exception ex)
            {
                SkaldPlugin.Log?.LogError($"[Skald] Error scanning nearby enemies: {ex}");
            }
            return result;
        }

        private static void ExecuteAnnouncement(string victimName, string victimSteamId, Vector3 position)
        {
            // Debounce duplicate events within 3 seconds for the same player
            lock (_announceLock)
            {
                if (_lastDeathAnnounced.TryGetValue(victimName, out DateTime lastTime))
                {
                    if ((DateTime.UtcNow - lastTime).TotalSeconds < 3.0)
                    {
                        SkaldPlugin.Log?.LogInfo($"[Skald][Hook] Death for {victimName} debounced (duplicate event within 3s).");
                        return;
                    }
                }
                _lastDeathAnnounced[victimName] = DateTime.UtcNow;
            }

            // Determine killer/category via proximity heuristic (no HitData available)
            DeathCategory category;
            string killerName;
            List<(string name, float dist)> nearbyEnemies = FindNearbyEnemies(position, KillerSearchRadius);
            int swarmCount = nearbyEnemies.Count(e => e.dist <= SwarmRadius);
            if (swarmCount > OverwhelmedThreshold)
            {
                category = DeathCategory.Overwhelmed;
                killerName = "a horde of enemies";
            }
            else if (nearbyEnemies.Count > 0)
            {
                string closest = nearbyEnemies.OrderBy(e => e.dist).First().name;
                (category, killerName) = DeathAnalyzer.ClassifyByEnemyName(closest);
            }
            else
            {
                category = DeathCategory.Generic;
                killerName = "The Fates";
            }

            // Determine biome
            string biomeName = "Wilds";
            try
            {
                if (WorldGenerator.instance != null)
                {
                    biomeName = WorldGenerator.instance.GetBiome(position.x, position.z).ToString();
                }
                else if (EnvMan.instance != null)
                {
                    biomeName = EnvMan.instance.GetCurrentBiome().ToString();
                }
            }
            catch { }

            // Format message
            string announcement = DeathAnalyzer.FormatDeathMessage(category, victimName, killerName, biomeName);

            // Record in history for WebPortal
            var record = new DeathEventRecord
            {
                VictimName = victimName,
                VictimSteamId = victimSteamId,
                KillerName = killerName,
                Category = category,
                Biome = biomeName,
                Position = position,
                FormattedMessage = announcement,
                Timestamp = DateTime.UtcNow
            };
            ChronicleRegistry.RecordDeath(record);

            if (ModConfig.LogToConsole.Value)
            {
                SkaldPlugin.Log?.LogInfo($"[Skald] 💀 {announcement} (Pos: {position})");
            }

            // Broadcast in-game shout and banner to all connected players
            BroadcastHelper.BroadcastDeathMessage(announcement, position);
        }
    }
}
