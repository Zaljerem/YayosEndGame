using HarmonyLib;
using RimWorld;

namespace yayoEnding;

[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
public class DefGenerator_GenerateImpliedDefs_PreResolve
{
    public static void Prefix(bool hotReload)
    {
        YayoEndingMod.DebugLogging("[YayoEnding] :: Generating Defs");

        YayoEndingMod.PatchDef();
        YayoEndingMod.DebugLogging("[YayoEnding] :: Applying Graphics");

        YayoEndingMod.UpdateGraphics();
    }
}