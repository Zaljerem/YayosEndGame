using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoEnding;

[HarmonyPatch(typeof(ThingFilter), nameof(ThingFilter.SetFromPreset))]
internal class ThingFilter_SetFromPreset
{
    private static void Postfix(ThingFilter __instance, StorageSettingsPreset preset)
    {
        if(preset == StorageSettingsPreset.DefaultStockpile)
        {
            __instance.SetAllow(ThingCategoryDef.Named("yy_gem_category"), true);
        }
    }
}