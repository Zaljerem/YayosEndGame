# Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose

This RimWorld mod enhances gameplay by introducing new building mechanics and in-game features. The core purpose of the mod is to provide players with additional layers of interaction and challenges by integrating teleportation technology and gem crafting capabilities.

## Key Features and Systems

1. **Teleportation System:**
   - Implemented using the `Building_Teleporter` class, which provides mechanics for fast travel and efficient logistics.
   - Key Methods:
     - `ForceLaunch()`: Instantly initiates teleportation.
     - `TryLaunch()`: Handles the logic of launching if conditions are met.

2. **Gem Crafting System:**
   - Managed through `CompGemMaker` and `CompProperties_GemMaker` classes, allowing players to craft and utilize gems.
   - Key Methods in `CompGemMaker`:
     - `DrillWorkDone(Pawn driller)`: Triggers when a pawn completes a drilling task.
     - `TryProducePortion()`: Handles the production of gem portions based on defined criteria.
     - `CanDrillNow()`: Checks whether drilling operations can commence.
     - `UsedLastTick()`: Checks if the drill was operational in the last game tick.

## Coding Patterns and Conventions

- **Class Naming:** Classes follow a naming convention that reflects their functionality, such as `Building_Teleporter` for teleportation and `CompGemMaker` for component-based gem crafting.
- **Method Visibility:** Most methods are kept private unless they are meant to be directly accessed or overridden, ensuring encapsulation and proper API exposure.
- **Static Classes:** Used for defining job and thing definitions (`JobDefOf`, `ThingDefOf`) for ease of access and centralized management.

## XML Integration

Although not detailed in your summary, XML is typically used in RimWorld modding for defining game data like items, buildings, and recipes. Ensure that:
- XML files are well-structured to define new items, recipes, and jobs.
- Def names in the XML are referenced correctly in the C# code for seamless integration.

## Harmony Patching

Harmony is extensively used in this mod for altering the base game behavior:

- **Patch Definitions:**
  - `harmonyPatch`: Base class for initializing patches.
  - `Patch_DefGenerator_GenerateImpliedDefs_PreResolve`: Alters the game's definition generation logic before resolution.
  - `patch_ShipCountdown_CountdownEnded`: Modifies the end behavior of the ship countdown.
  - `patch_ThingFilter_SetFromPreset`: Changes how thing filters are established from presets.

- **Internal vs Public:**
  - Internal patches (`patch_ThingFilter_SetFromPreset`) are typically less exposed unless they're part of a larger public API.

## Suggestions for Copilot

- **Autocomplete for Methods:** Focus on suggesting method signatures and inline comments to explain parameter roles and method purposes.
- **Harmony Patch Templates:** Generate templates for new Harmony patches with `Prefix`, `Postfix`, and `Transpiler` methods where applicable.
- **XML Handling:** Enhance suggestions for creating well-formed XML snippets for item and building definitions.
- **Debugging Helpers:** Offer snippets for basic logging and debugging, particularly within Harmony patches to inspect pre- and post-patch behaviors.

By following these instructions, you can effectively leverage GitHub Copilot to streamline your development process, maintain clean code, and ensure seamless interaction with RimWorld's mechanics.
