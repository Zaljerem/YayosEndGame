using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoEnding;

[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
public class DefGenerator_GenerateImpliedDefs_PreResolve
{
    public static void Prefix(bool hotReload)
    {
        YayoEndingMod.DebugLogging("[YayoEnding] :: Generating Defs");

        YayoEndingMod.Instance.PatchDef();
        YayoEndingMod.DebugLogging("[YayoEnding] :: Applying Graphics");

        YayoEndingMod.Instance.UpdateGraphics();
    }
}