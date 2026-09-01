using System;
using BepInEx;
using BepInEx.Logging;
using Skald.Configuration;
using Skald.Patches;

namespace Skald
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class SkaldPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.bigai.skald_vikingkillfeed";
        public const string PluginName = "Skald_VikingKillFeed";
        public const string PluginVersion = "1.0.0";

        public static SkaldPlugin Instance { get; private set; } = null!;
        public static ManualLogSource Log { get; private set; } = null!;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            try
            {
                Log.LogInfo($"══════════════════════════════════════════");
                Log.LogInfo($"  {PluginName} v{PluginVersion} loading...");
                Log.LogInfo($"══════════════════════════════════════════");

                // 1. Initialize configuration
                ModConfig.Initialize(Config);

                Log.LogInfo($"[{PluginName}] Server chronicle & Viking killfeed active.");

                // 2. Start the health-poll death detector (server-side, no client mod needed).
                InvokeRepeating(nameof(PollDeaths), 2f, 1f);
            }
            catch (Exception ex)
            {
                Log.LogError($"[{PluginName}] Failed to initialize: {ex}");
            }
        }

        private void PollDeaths()
        {
            try
            {
                PlayerDeathPatch.PollForDeaths();
            }
            catch (Exception ex)
            {
                Log.LogError($"[{PluginName}] Error during health poll: {ex}");
            }
        }

        private void OnDestroy()
        {
            Log.LogInfo($"[{PluginName}] Unloaded.");
        }
    }
}
