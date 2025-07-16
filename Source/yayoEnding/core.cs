using System.Collections.Generic;
using System.Linq;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoEnding;

public class core : ModBase
{
    private static readonly List<string> arGemDef = [];
    private static int goalBiome = 2;
    public static float ExtractSpeed = 1f;
    private static bool ignoreExtreme;

    private SettingHandle<float> extractSpeedSetting;

    private SettingHandle<int> goalBiomeSetting;

    private SettingHandle<bool> ignoreExtremeSetting;
    public override string ModIdentifier => "yayoEnding";


    public override void DefsLoaded()
    {
        // 셋팅
        goalBiomeSetting =
            Settings.GetHandle("goalBiome", "goalBiome_title".Translate(), "goalBiome_desc".Translate(), 2);
        goalBiome = goalBiomeSetting.Value;

        ignoreExtremeSetting =
            Settings.GetHandle("ignoreExtreme", "yayoEnding_ExcludeExtreme".Translate(),
                "yayoEnding_ExcludeExtremeTT".Translate(), false);
        ignoreExtreme = ignoreExtremeSetting.Value;

        extractSpeedSetting = Settings.GetHandle("extractSpeed", "extractSpeed_title".Translate(),
            "extractSpeed_desc".Translate(), 1f);
        ExtractSpeed = extractSpeedSetting.Value;


        // 바이옴 에너지 조각 텍스쳐 수정
        var a = 0;
        foreach (var t in from thing in DefDatabase<ThingDef>.AllDefs
                 where
                     thing.defName.Contains("yy_gem_")
                 select thing)
        {
            var gd = new GraphicData
            {
                graphicClass = typeof(Graphic_Single),
                texPath = $"yy_bep{a % 15}"
            };

            t.graphicData = gd;
            a++;
        }
    }

    public override void SettingsChanged()
    {
        goalBiome = goalBiomeSetting.Value;
        ExtractSpeed = Mathf.Clamp(extractSpeedSetting.Value, 0.01f, 50f);
    }


    public static void patchDef()
    {
        Log.Message("# Yayo's Ending Init 1");

        Log.Message("# generate biome energy item");


        foreach (var b in from biome in DefDatabase<BiomeDef>.AllDefs
                 where !biome.impassable && (ignoreExtreme || !biome.isExtremeBiome)
                 select biome)
        {
            var t = new ThingDef
            {
                // base
                thingClass = typeof(ThingWithComps),
                category = ThingCategory.Item,
                resourceReadoutPriority = ResourceCountPriority.Middle,
                selectable = true,
                altitudeLayer = AltitudeLayer.Item,
                comps = [new CompProperties_Forbiddable()],
                alwaysHaulable = true,
                drawGUIOverlay = true,
                rotatable = false,
                pathCost = 14,
                // detail
                defName = $"yy_gem_{b.defName}",
                label = string.Format("yayoEnding_energyPiece".Translate(), b.label),
                description = string.Format("yayoEnding_energyPiece".Translate(), b.label),
                graphicData = new GraphicData
                {
                    texPath = "Things/Item/Resource/Gold",
                    graphicClass = typeof(Graphic_StackCount)
                },
                soundInteract = SoundDef.Named("Silver_Drop"),
                soundDrop = SoundDef.Named("Silver_Drop"),
                useHitPoints = false,
                healthAffectsPrice = false,
                statBases = []
            };

            t.statBases = RimWorld.ThingDefOf.Silver.statBases;
            t.thingCategories = [];

            t.stackLimit = 100;
            t.burnableByRecipe = false;
            t.smeltable = false;
            t.terrainAffordanceNeeded = TerrainAffordanceDefOf.Medium;

            t.thingCategories.Add(ThingCategoryDef.Named("yy_gem_piece_category"));
            t.tradeability = Tradeability.None;
            t.tradeTags = ["yy_gem"];

            arGemDef.Add(t.defName);
            DefGenerator.AddImpliedDef(t);
        }

        patchDef2();
    }


    private static void patchDef2()
    {
        Log.Message("# Yayo's Ending Init 2");

        Log.Message("# generate planet energy core recipe");
        for (var i = 0; i < 4; i++)
        {
            var r = new RecipeDef
            {
                defName = $"Make_yy_planetCore_{i + 1}",
                label = string.Format("yayoEnding_energyCore_recipe_label".Translate(),
                    ThingDef.Named("yy_planetCore").label, (i + 1).ToString()),
                description = ThingDef.Named("yy_planetCore").description,
                jobString = string.Format("yayoEnding_energyCore_recipe_jobstring".Translate(),
                    ThingDef.Named("yy_planetCore").label),
                workSpeedStat = StatDefOf.GeneralLaborSpeed,
                effectWorking = EffecterDefOf.Drill,
                soundWorking = SoundDef.Named("Recipe_Machining"),
                workAmount = 1500, // 작업량
                recipeUsers =
                [
                    ThingDef.Named("CraftingSpot"),
                    ThingDef.Named("FueledSmithy"),
                    ThingDef.Named("ElectricSmithy"),
                    ThingDef.Named("TableMachining"),
                    ThingDef.Named("FabricationBench")
                ], // 제작 장소
                unfinishedThingDef = ThingDef.Named("UnfinishedComponent")
            };

            var ingredient = new List<IngredientCount>();
            while (ingredient.Count < goalBiome && ingredient.Count < arGemDef.Count)
            {
                var td = ThingDef.Named(arGemDef[Rand.Range(0, arGemDef.Count)]);
                var already = false;
                foreach (var ingredientCount in ingredient)
                {
                    if (ingredientCount.filter.AllowedThingDefs.Contains(td))
                    {
                        already = true;
                    }
                }

                if (already)
                {
                    continue;
                }

                var ing = new IngredientCount();

                ing.filter.SetAllow(td, true);

                ing.SetBaseCount(100); // 각 바이옴 조각 필요 개수
                ingredient.Add(ing);
            }

            r.ingredients = ingredient;
            //Log.Message($"{r.ingredients[0].filter.AnyAllowedDef.defName}");


            r.products = [];
            var tdc = new ThingDefCountClass
            {
                thingDef = ThingDef.Named("yy_planetCore"),
                count = 1
            };
            r.products.Add(tdc);
            r.skillRequirements = [];
            var sr = new SkillRequirement
            {
                skill = SkillDefOf.Crafting,
                minLevel = 8 // 제작 스킬레벨
            };
            r.skillRequirements.Add(sr);
            r.workSkill = SkillDefOf.Crafting;

            //DefDatabase<RecipeDef>.Add(r);
            DefGenerator.AddImpliedDef(r);
        }
    }


    public override void WorldLoaded()
    {
        base.WorldLoaded();


        var seed = Find.World.info.Seed;

        // 실존하는 바이옴 리스트 생성
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

        // 행성 에너지 핵 레시피 수정
        foreach (var r in from recipe in DefDatabase<RecipeDef>.AllDefs
                 where
                     recipe.defName.Contains("Make_yy_planetCore_")
                 select recipe)
        {
            var ingredient = new List<IngredientCount>();
            while (ingredient.Count < goalBiome && ingredient.Count < tmp_ar_gemDef.Count)
            {
                var td = ThingDef.Named(tmp_ar_gemDef[Rand.RangeSeeded(0, tmp_ar_gemDef.Count, seed)]);
                seed++;

                var already = false;
                foreach (var ingredientCount in ingredient)
                {
                    if (ingredientCount.filter.AllowedThingDefs.Contains(td))
                    {
                        already = true;
                    }
                }

                if (already)
                {
                    continue;
                }

                var ing = new IngredientCount();

                ing.filter.SetAllow(td, true);

                ing.SetBaseCount(100); // 각 바이옴 조각 필요 개수
                ingredient.Add(ing);
            }

            r.ingredients = ingredient;
        }
    }
}