using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace yayoEnding;

// WorldComponent to handle per-world updates originally in WorldLoaded()
public class YayoEndingWorldComponent : WorldComponent
{
#pragma warning disable IDE0290
    public YayoEndingWorldComponent(World world) : base(world)
#pragma warning restore IDE0290
    {
    }

    // FinalizeInit is called after the world is loaded and ready; this approximates ModBase.WorldLoaded()
    public override void FinalizeInit(bool fromLoad)
    {
        base.FinalizeInit(fromLoad);
        UpdatePlanetCoreRecipes();
    }

    private static void UpdatePlanetCoreRecipes()
    {
        YayoEndingMod.DebugLogging("[YayoEnding] :: WorldLoaded - updating planet core recipes");

        // seeded RNG matching original behaviour
        var seed = Find.World.info.Seed;

        // collect gem defs that exist on tiles and are buildable
        var tmp_ar_gemDef = new List<string>();
        foreach (var tile in Find.WorldGrid.Tiles)
        {
            var gemDefName = $"yy_gem_{tile.PrimaryBiome.defName}";
            if (DefDatabase<ThingDef>.GetNamedSilentFail(gemDefName) == null)
            {
                continue;
            }

            if (tile.PrimaryBiome.canBuildBase && !tmp_ar_gemDef.Contains(gemDefName))
            {
                tmp_ar_gemDef.Add(gemDefName);
            }
        }

        // update recipe ingredients that match "Make_yy_planetCore_"
        foreach (var r in from recipe in DefDatabase<RecipeDef>.AllDefs
                 where recipe.defName.Contains("Make_yy_planetCore_")
                 select recipe)
        {
            var ingredient = new List<IngredientCount>();
            while (ingredient.Count < YayoEndingMod.goalBiome && ingredient.Count < tmp_ar_gemDef.Count)
            {
                var td = ThingDef.Named(tmp_ar_gemDef[Rand.RangeSeeded(0, tmp_ar_gemDef.Count, seed)]);
                seed++;

                var already = ingredient.Any(ic => ic.filter.AllowedThingDefs.Contains(td));
                if (already)
                {
                    continue;
                }

                var ing = new IngredientCount();
                ing.filter.SetAllow(td, true);
                ing.SetBaseCount(100);
                ingredient.Add(ing);
            }

            r.ingredients = ingredient;
        }
    }
}