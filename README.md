# Skald_VikingKillFeed

> **Skald_VikingKillFeed is a lightweight mod designed to broadcast immersive Viking death announcements, killfeed chronicles, and boss alerts across the server.**

### About the Name: *Skald*
In Viking society, a **Skald** was an honored poet, chronicler, and storyteller tasked with composing oral histories that immortalized epic deeds, great battles, and the honorable demises of warriors. In this mod, the *Skald* serves as your server's herald—announcing combat deaths, boss victories, and heroic last stands in real time.

---

## Features
- **Context-Aware Death Announcements**: Broadcasts server-wide immersive death messages with biome, hostile creature, and star rank context.
- **Boss & Horde Detection**: Identifies legendary boss battles and horde last stands with customized flavor pools.
- **Customizable Message Templates**: Personalize announcement pools using dynamic tokens (`{victim}`, `{killer}`, `{biome}`).
- **Server-Side Operation**: Runs server-side with native Valheim chat broadcasts; vanilla clients do not need the mod installed.

---

### Installation Type
- **Location:** Server-side (or local host for singleplayer). Clients do not need the mod installed.
- **Enforcement:** Optional on clients; broadcasts work on vanilla clients.

### Manual Install
1. Ensure BepInEx is installed on your server.
2. Extract the downloaded `.zip` archive.
3. Copy `Skald_VikingKillFeed.dll` into your `Valheim/BepInEx/plugins/` folder.
4. Launch the game once to generate the default configuration file.

---

## Configuration
The configuration file is automatically created at `BepInEx/config/com.bigai.skald_vikingkillfeed.cfg` after running the game once.

| Section | Setting | Default | Description |
| :--- | :--- | :--- | :--- |
| `1 - General` | `EnableDeathAnnouncements` | `true` | Enable in-game global announcements when a player dies. |
| `1 - General` | `EnableBossDefeatAnnouncements` | `true` | Broadcast server-wide when a legendary boss is summoned or defeated. |
| `1 - General` | `IncludeBiomeInMessage` | `true` | Include the biome name (e.g., Meadows, Swamp, Mistlands) in the announcement. |
| `1 - General` | `LogToConsole` | `true` | Output all death records and chronicles to the server console log. |
| `2 - Templates` | `MonsterDeathMessages` | `{victim} was slain by a {killer} in the {biome};...` | Semicolon-separated templates for monster deaths. Tokens: `{victim}`, `{killer}`, `{biome}` |
| `2 - Templates` | `BossDeathMessages` | `{victim} was annihilated by the mythical {killer}!;...` | Semicolon-separated templates for boss deaths. Tokens: `{victim}`, `{killer}`, `{biome}` |
| `2 - Templates` | `OverwhelmedMessages` | `{victim} was defeated in glorious battle...;...` | Semicolon-separated templates for deaths surrounded by multiple enemies. Tokens: `{victim}`, `{biome}` |
| `2 - Templates` | `GenericDeathMessages` | `{victim} has departed for the halls of Valhalla;...` | Fallback generic death templates. Tokens: `{victim}`, `{biome}` |

---

## Controls & Commands
- **Keybinds:** None.
- **Admin Commands:** None.

---

## Compatibility & Safe Removal
- **Multiplayer:** Server-authoritative. Vanilla clients can join and receive death announcements without having the mod installed.
- **Save Integrity:** Safe to install or remove at any time on existing servers and worlds without affecting save files.

### AI Disclosure 

I made this mod using AI. Most of the code in this mod was AI generated. If you have an issue with this, I completely understand and urge you to not use this mod. This mod ("Skald_VikingKillFeed") is meant as a lightweight mod for small servers that don't need all the bells and whistles of a more complex mod.
