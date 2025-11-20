# GitHub Copilot Instructions for Yayo's EndGame (Continued) Mod Project

## Mod Overview and Purpose

**Mod Name:** Yayo's EndGame (Continued)

Yayo's EndGame (Continued) is an update of Latki, YAYO's original mod designed to introduce a new ending to RimWorld. The mod provides a story-driven reason for players to embark on an epic journey across multiple biomes, enriching the gaming experience by requiring players to travel and gather resources across diverse environments. Players extract biome energy shards, craft a planet energy core, and finally build a planet energy teleporter to reach the unknown instigators of this interstellar saga.

## Key Features and Systems

1. **Energy Shard Extraction:**
   - Players extract 100 biome energy shards from three different biomes. The type of shard depends on the biome and the number of biomes can be modified via settings.

2. **Crafting the Planet Energy Core:**
   - The core is a critical piece for building energy teleporters. Its creation requires biome energy shards, with the specific types of shards required being randomly determined.

3. **Constructing the Planet Energy Teleporter:**
   - An important goal of the mod is to build this teleporter, a mysterious device powered by energy shards, capable of transporting users to an unknown destination.

4. **15-Day Raid Defense:**
   - Following the teleporter's completion, players must endure a 15-day raid defense sequence before making their escape and triggering a new ending.

5. **New Ending:**
   - Provides a fresh concluding narrative for players who complete the journey.

## Coding Patterns and Conventions

- The project uses C# coding conventions typical of RimWorld modding, focusing on clarity and maintainability. Classes are well-named to reflect their purpose and functionality.
- Method names clearly express their intended actions, like `forceLaunch`, `tryLaunch`, `DrillWorkDone`, and `UpdateGraphics`.
- Consistent use of access levels helps maintain encapsulation, especially using `public`, `private`, and `internal`.

## XML Integration

- XML is predominantly utilized for defining in-game elements, settings, and configurations. It's advisable to refer to existing XML files in the RimWorld mod directory for schema examples.
- Use XML attributes for mod settings and game definitions, ensuring changes are reflected in-game.

## Harmony Patching

- **Harmony**: The mod uses Harmony for runtime method patching to extend or modify the game's existing behavior.
- Example patches include `patch_ThingFilter_SetFromPreset` to modify filter settings and `ShipCountdown_CountdownEnded` to re-define the conditions when a ship countdown ends.
- For new patches, ensure proper `Prefix`, `Postfix`, or `Transpiler` usage according to desired alterations.

## Suggestions for Copilot

- **Biomes and Shards**: Suggest exploration of alternative biome types or configurations that affect the types and amounts of energy shards generated.
- **Plan Escape Scenarios**: Consider suggesting variations on the escape sequence, such as different raid types or additional end-game challenges.
- **XML File Suggestions**: Help create or refine XML definitions that integrate directly with the mod’s features, especially new game settings or item definitions.
- **Functionality Extensions**: Suggest ways to extend or enhance existing `Comp` classes like `CompGemMaker`, possibly for additional gameplay mechanics or interactive elements.
- **Testing Scenarios**: Generate unit test templates to ensure robustness of the components like `Building_Teleporter` or `CompGemMaker`.

The mod encourages community contributions and variations, emphasizing flexibility and creativity in extending its base features. Developers proficient in C# are invited to explore and expand the mod's potential.

--- 

Use the provided contact information for direct support and development discussions, especially via dedicated RimWorld Discord channels.
