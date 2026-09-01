using System;
using System.Text.RegularExpressions;
using Skald.Configuration;

namespace Skald.Logic
{
    public static class DeathAnalyzer
    {
        private static readonly System.Random _rng = new System.Random();

        private static readonly string[] BossNames = new[]
        {
            "Eikthyr", "Elder", "The Elder", "Bonemass", "Moder", "Yagluth", 
            "The Queen", "Queen", "Fader", "Fader, Lord of the Ashlands"
        };

        public static string FormatDeathMessage(DeathCategory category, string victimName, string killerName, string biome)
        {
            string templateConfig = category switch
            {
                DeathCategory.Boss => ModConfig.BossDeathMessages.Value,
                DeathCategory.Monster => ModConfig.MonsterDeathMessages.Value,
                DeathCategory.Overwhelmed => ModConfig.OverwhelmedMessages.Value,
                _ => ModConfig.GenericDeathMessages.Value,
            };

            string[] pool = ModConfig.GetTemplates(templateConfig);
            string chosenTemplate = pool[_rng.Next(pool.Length)];

            string result = chosenTemplate
                .Replace("{victim}", victimName)
                .Replace("{killer}", killerName)
                .Replace("{biome}", biome);

            return result;
        }

        // Classifies an already-identified enemy name (e.g. from a proximity scan
        // with no HitData available) into the same Boss/Monster categories used
        // for hook-based detection.
        public static (DeathCategory category, string killerName) ClassifyByEnemyName(string name)
        {
            string cleaned = CleanEntityName(name);
            return IsBoss(cleaned) ? (DeathCategory.Boss, cleaned) : (DeathCategory.Monster, cleaned);
        }

        private static bool IsBoss(string name)
        {
            foreach (var b in BossNames)
            {
                if (name.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        public static string CleanEntityName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Creature";
            // Remove localization tokens if raw is something like "$enemy_troll"
            if (raw.StartsWith("$"))
            {
                raw = raw.Substring(1);
            }
            raw = Regex.Replace(raw, @"^enemy_", "", RegexOptions.IgnoreCase);
            raw = Regex.Replace(raw, @"\(Clone\)$", "");
            return raw.Trim();
        }
    }
}
