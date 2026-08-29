using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skald.Logic
{
    public enum DeathCategory
    {
        Pvp,
        Boss,
        Monster,
        FallingTree,
        Drowning,
        Freezing,
        Burning,
        Poison,
        FallDamage,
        Generic
    }

    public class DeathEventRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string VictimName { get; set; } = string.Empty;
        public string VictimSteamId { get; set; } = string.Empty;
        public string KillerName { get; set; } = string.Empty;
        public DeathCategory Category { get; set; }
        public string Biome { get; set; } = string.Empty;
        public Vector3 Position { get; set; }
        public string FormattedMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public static class ChronicleRegistry
    {
        private static readonly List<DeathEventRecord> _history = new List<DeathEventRecord>();
        private static readonly object _lock = new object();
        private const int MaxHistoryCount = 200;

        public static void RecordDeath(DeathEventRecord record)
        {
            lock (_lock)
            {
                _history.Add(record);
                if (_history.Count > MaxHistoryCount)
                {
                    _history.RemoveAt(0);
                }
            }
        }

        public static List<DeathEventRecord> GetRecentDeaths(int count = 50)
        {
            lock (_lock)
            {
                var result = new List<DeathEventRecord>(_history);
                if (result.Count > count)
                {
                    return result.GetRange(result.Count - count, count);
                }
                return result;
            }
        }
    }
}
