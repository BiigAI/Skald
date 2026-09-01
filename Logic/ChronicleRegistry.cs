using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skald.Logic
{
    public enum DeathCategory
    {
        Boss,
        Monster,
        Overwhelmed,
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

            // Sync with ValheimPortal WebApiRouter if active in AppDomain
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string asmName = asm.GetName().Name;
                    if (asmName == "Bifrostheim" || asmName == "ValheimPortal")
                    {
                        var routerType = asm.GetType("ValheimPortal.Systems.Web.WebApiRouter");
                        var dtoType = asm.GetType("ValheimPortal.Systems.Web.SkaldDeathRecordDto");
                        if (routerType != null && dtoType != null)
                        {
                            var field = routerType.GetField("_skaldChronicle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            if (field != null)
                            {
                                var list = field.GetValue(null);
                                if (list != null)
                                {
                                    var dto = Activator.CreateInstance(dtoType);
                                    dtoType.GetProperty("id")?.SetValue(dto, record.Id, null);
                                    dtoType.GetProperty("victimName")?.SetValue(dto, record.VictimName, null);
                                    dtoType.GetProperty("victimSteamId")?.SetValue(dto, record.VictimSteamId, null);
                                    dtoType.GetProperty("killerName")?.SetValue(dto, record.KillerName, null);
                                    dtoType.GetProperty("category")?.SetValue(dto, record.Category.ToString(), null);
                                    dtoType.GetProperty("biome")?.SetValue(dto, record.Biome, null);
                                    dtoType.GetProperty("formattedMessage")?.SetValue(dto, record.FormattedMessage, null);
                                    dtoType.GetProperty("timestamp")?.SetValue(dto, record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), null);

                                    var insertMethod = list.GetType().GetMethod("Insert");
                                    insertMethod?.Invoke(list, new object[] { 0, dto });
                                }
                            }
                        }
                    }
                }
            }
            catch { }
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
