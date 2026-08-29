using System;
using System.Text.RegularExpressions;
using Skald.Configuration;
using UnityEngine;

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

        public static (DeathCategory category, string killerName) AnalyzeDeath(Player victim, HitData? lastHit)
        {
            if (victim == null)
                return (DeathCategory.Generic, "Unknown");

            // 1. Check if victim is in water and out of stamina (Drowning)
            if (victim.InWater() && victim.GetStamina() <= 1f && (lastHit == null || lastHit.m_damage.GetTotalDamage() <= 0.1f))
            {
                return (DeathCategory.Drowning, "Water");
            }

            // 2. Check if freezing / cold environment
            if (victim.IsFreezing() && (lastHit == null || lastHit.m_damage.m_frost > 0 || lastHit.m_damage.GetTotalDamage() <= 0.1f))
            {
                return (DeathCategory.Freezing, "Bitter Cold");
            }

            // If no hit data is present, evaluate generic/fall
            if (lastHit == null)
            {
                return (DeathCategory.Generic, "The Fates");
            }

            // 3. Check fall damage
            if (lastHit.m_damage.m_damage > 0 && lastHit.m_attacker == ZDOID.None && lastHit.m_hitType == HitData.HitType.None)
            {
                return (DeathCategory.FallDamage, "Gravity");
            }

            // 4. Check attacker entity
            Character? attackerChar = lastHit.GetAttacker();
            if (attackerChar != null)
            {
                string attackerName = attackerChar.GetHoverName();
                if (string.IsNullOrWhiteSpace(attackerName))
                {
                    attackerName = attackerChar.m_name ?? "Creature";
                }
                attackerName = CleanEntityName(attackerName);

                // Is the killer another player (PvP)?
                if (attackerChar is Player killerPlayer && killerPlayer != victim)
                {
                    string pvpKillerName = killerPlayer.GetPlayerName();
                    if (string.IsNullOrWhiteSpace(pvpKillerName)) pvpKillerName = attackerName;
                    return (DeathCategory.Pvp, pvpKillerName);
                }

                // Is the killer a boss?
                if (IsBoss(attackerName))
                {
                    return (DeathCategory.Boss, attackerName);
                }

                // Check star level
                int level = attackerChar.GetLevel();
                if (level == 2)
                {
                    attackerName = "1-Star " + attackerName;
                }
                else if (level >= 3)
                {
                    attackerName = "2-Star " + attackerName;
                }

                return (DeathCategory.Monster, attackerName);
            }

            // 5. Check if falling tree log / tree damage
            if (lastHit.m_hitType == HitData.HitType.Tree || lastHit.m_damage.m_blunt > 40f && lastHit.m_attacker == ZDOID.None)
            {
                return (DeathCategory.FallingTree, "Falling Log");
            }

            // 6. Check specific elemental damage types
            if (lastHit.m_damage.m_fire > lastHit.m_damage.GetTotalPhysicalDamage() && lastHit.m_damage.m_fire > 5f)
            {
                return (DeathCategory.Burning, "Fire");
            }

            if (lastHit.m_damage.m_poison > lastHit.m_damage.GetTotalPhysicalDamage() && lastHit.m_damage.m_poison > 5f)
            {
                return (DeathCategory.Poison, "Poison");
            }

            if (lastHit.m_damage.m_frost > lastHit.m_damage.GetTotalPhysicalDamage() && lastHit.m_damage.m_frost > 5f)
            {
                return (DeathCategory.Freezing, "Frost");
            }

            return (DeathCategory.Generic, "The Wilds");
        }

        public static string FormatDeathMessage(DeathCategory category, string victimName, string killerName, string biome)
        {
            string templateConfig = category switch
            {
                DeathCategory.Pvp => ModConfig.PvpDeathMessages.Value,
                DeathCategory.Boss => ModConfig.BossDeathMessages.Value,
                DeathCategory.Monster => ModConfig.MonsterDeathMessages.Value,
                DeathCategory.FallingTree => ModConfig.FallingTreeMessages.Value,
                DeathCategory.Drowning => ModConfig.DrowningMessages.Value,
                DeathCategory.Freezing => ModConfig.FreezingMessages.Value,
                DeathCategory.Burning => ModConfig.BurningMessages.Value,
                DeathCategory.Poison => ModConfig.PoisonMessages.Value,
                DeathCategory.FallDamage => ModConfig.FallDamageMessages.Value,
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

        private static bool IsBoss(string name)
        {
            foreach (var b in BossNames)
            {
                if (name.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static string CleanEntityName(string raw)
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
