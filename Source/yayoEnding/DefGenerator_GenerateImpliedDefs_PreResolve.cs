using HarmonyLib;
using RimWorld;

namespace yayoEnding;

[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
public class DefGenerator_GenerateImpliedDefs_PreResolve
{
    public static void Prefix()
    {
        core.patchDef();
    }
}