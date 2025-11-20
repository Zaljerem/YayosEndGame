using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoEnding;

[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
public class DefGenerator_GenerateImpliedDefs_PreResolve
{
    public static bool Prefix(bool hotReload)
    {
        if (YayoEndingMod.DebugLogging)
        {
            Log.Message("[YayoEnding] :: Generating Defs");
        }
        YayoEndingMod.Instance.PatchDef();
        if (YayoEndingMod.DebugLogging)
        {
            Log.Message("[YayoEnding] :: Applying Graphics");
        }
        YayoEndingMod.Instance.UpdateGraphics();

        return true;
    }
}