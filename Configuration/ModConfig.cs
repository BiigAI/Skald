using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Skald.Configuration
{
    public static class ModConfig
    {
        // General
        public static ConfigEntry<bool> EnableDeathAnnouncements { get; private set; } = null!;
        public static ConfigEntry<bool> EnableBossDefeatAnnouncements { get; private set; } = null!;
        public static ConfigEntry<bool> IncludeBiomeInMessage { get; private set; } = null!;
        public static ConfigEntry<bool> LogToConsole { get; private set; } = null!;

        // Custom template pools
        public static ConfigEntry<string> MonsterDeathMessages { get; private set; } = null!;
        public static ConfigEntry<string> BossDeathMessages { get; private set; } = null!;
        public static ConfigEntry<string> OverwhelmedMessages { get; private set; } = null!;
        public static ConfigEntry<string> GenericDeathMessages { get; private set; } = null!;

        public static void Initialize(ConfigFile config)
        {
            // ── Section 1: General ─────────────────────────────────────────────
            EnableDeathAnnouncements = config.Bind(
                "1 - General",
                "EnableDeathAnnouncements",
                true,
                "Enable in-game global announcements when a player dies."
            );

            EnableBossDefeatAnnouncements = config.Bind(
                "1 - General",
                "EnableBossDefeatAnnouncements",
                true,
                "Broadcast server-wide when a legendary boss is summoned or defeated."
            );

            IncludeBiomeInMessage = config.Bind(
                "1 - General",
                "IncludeBiomeInMessage",
                true,
                "Include the biome name (e.g. Meadows, Swamp, Mistlands) in the announcement."
            );

            LogToConsole = config.Bind(
                "1 - General",
                "LogToConsole",
                true,
                "Output all death records and chronicles to the server console log."
            );

            // ── Section 2: Flavor Templates (Semicolon-separated) ───────────────
            MonsterDeathMessages = config.Bind(
                "2 - Templates",
                "MonsterDeathMessages",
                "{victim} was slain by a {killer} in the {biome};{victim} was torn apart by a {killer};{victim} fell before the wrath of a {killer} in the {biome};A {killer} claimed the soul of {victim}",
                "Message templates for monster deaths. Semicolon-separated. Tokens: {victim}, {killer}, {biome}"
            );

            BossDeathMessages = config.Bind(
                "2 - Templates",
                "BossDeathMessages",
                "{victim} was annihilated by the mythical {killer}!;The legendary {killer} crushed {victim} into dust;{victim} dared challenge {killer} and was destroyed",
                "Message templates for boss deaths. Semicolon-separated. Tokens: {victim}, {killer}, {biome}"
            );

            OverwhelmedMessages = config.Bind(
                "2 - Templates",
                "OverwhelmedMessages",
                "{victim} was defeated in glorious battle against a horde in the {biome};{victim} fell fighting valiantly against overwhelming odds;{victim} made a last stand against a swarm of enemies and was overwhelmed",
                "Message templates for deaths surrounded by multiple enemies. Semicolon-separated. Tokens: {victim}, {biome}"
            );

            GenericDeathMessages = config.Bind(
                "2 - Templates",
                "GenericDeathMessages",
                "{victim} has departed for the halls of Valhalla;The Norns have cut the thread of {victim}'s life;{victim} died in the {biome}",
                "Fallback generic death templates. Semicolon-separated. Tokens: {victim}, {biome}"
            );
        }

        public static string[] GetTemplates(string configValue)
        {
            if (string.IsNullOrWhiteSpace(configValue))
                return new[] { "{victim} has died in the {biome}." };

            return configValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
