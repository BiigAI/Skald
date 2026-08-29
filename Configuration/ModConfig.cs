using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Skald.Configuration
{
    public static class ModConfig
    {
        // General
        public static ConfigEntry<bool> EnableDeathAnnouncements { get; private set; } = null!;
        public static ConfigEntry<bool> EnablePvpAnnouncements { get; private set; } = null!;
        public static ConfigEntry<bool> EnableBossDefeatAnnouncements { get; private set; } = null!;
        public static ConfigEntry<bool> IncludeBiomeInMessage { get; private set; } = null!;
        public static ConfigEntry<bool> LogToConsole { get; private set; } = null!;

        // Custom template pools
        public static ConfigEntry<string> MonsterDeathMessages { get; private set; } = null!;
        public static ConfigEntry<string> BossDeathMessages { get; private set; } = null!;
        public static ConfigEntry<string> FallingTreeMessages { get; private set; } = null!;
        public static ConfigEntry<string> DrowningMessages { get; private set; } = null!;
        public static ConfigEntry<string> FreezingMessages { get; private set; } = null!;
        public static ConfigEntry<string> BurningMessages { get; private set; } = null!;
        public static ConfigEntry<string> PoisonMessages { get; private set; } = null!;
        public static ConfigEntry<string> FallDamageMessages { get; private set; } = null!;
        public static ConfigEntry<string> PvpDeathMessages { get; private set; } = null!;
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

            EnablePvpAnnouncements = config.Bind(
                "1 - General",
                "EnablePvpAnnouncements",
                true,
                "Enable dedicated announcements when a player is killed by another player in PvP."
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

            FallingTreeMessages = config.Bind(
                "2 - Templates",
                "FallingTreeMessages",
                "{victim} was crushed by a falling log!;{victim} learned that lumberjacking is the most dangerous trade in Valheim;The wrath of the forest felled {victim} with a falling tree",
                "Message templates for falling tree deaths. Semicolon-separated. Tokens: {victim}, {killer}, {biome}"
            );

            DrowningMessages = config.Bind(
                "2 - Templates",
                "DrowningMessages",
                "{victim} ran out of stamina and drowned in the cold waters;The sea claimed {victim} to Ran's watery depths;{victim} sank to the bottom of the {biome} waters",
                "Message templates for drowning deaths. Semicolon-separated. Tokens: {victim}, {biome}"
            );

            FreezingMessages = config.Bind(
                "2 - Templates",
                "FreezingMessages",
                "{victim} froze to death in the merciless blizzard of the {biome};The biting cold conquered {victim}'s spirit;{victim} turned into an icy statue in the {biome}",
                "Message templates for freezing deaths. Semicolon-separated. Tokens: {victim}, {biome}"
            );

            BurningMessages = config.Bind(
                "2 - Templates",
                "BurningMessages",
                "{victim} burned to ashes in the {biome};The flames consumed {victim};{victim} succumbed to searing fire",
                "Message templates for fire/burning deaths. Semicolon-separated. Tokens: {victim}, {biome}"
            );

            PoisonMessages = config.Bind(
                "2 - Templates",
                "PoisonMessages",
                "{victim} succumbed to deadly toxins in the {biome};Vile venom ended {victim}'s journey;{victim}'s veins were filled with lethal poison",
                "Message templates for poison deaths. Semicolon-separated. Tokens: {victim}, {biome}"
            );

            FallDamageMessages = config.Bind(
                "2 - Templates",
                "FallDamageMessages",
                "{victim} plummeted to their death from high cliffs;Gravity showed no mercy to {victim};{victim} took a fatal leap in the {biome}",
                "Message templates for fall damage deaths. Semicolon-separated. Tokens: {victim}, {biome}"
            );

            PvpDeathMessages = config.Bind(
                "2 - Templates",
                "PvpDeathMessages",
                "{victim} was vanquished by {killer} in glorious combat!;{killer} struck down {victim} with honor;The blade of {killer} claimed the life of {victim}",
                "Message templates for PvP combat deaths. Semicolon-separated. Tokens: {victim}, {killer}, {biome}"
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
