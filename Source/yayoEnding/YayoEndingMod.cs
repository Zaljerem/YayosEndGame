using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace yayoEnding
{
    public class YayoEndingMod : Mod
    {
        
        public static readonly List<string> arGemDef = new List<string>();
        public static int goalBiome = 2;
        public static float ExtractSpeed = 1f;
        public static bool ignoreExtreme = false;

        public YayoEndingSettings settings;

        public static YayoEndingMod Instance;

        public YayoEndingMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<YayoEndingSettings>();

            Instance = this;

            // apply persisted settings into static fields
            ApplySettingsToStatics();

            if (YayoEndingMod.DebugLogging)
            {
                Log.Message("[YayoEnding] :: Harmony patching");
            }
            new Harmony("yayoEnding").PatchAll();

            // This approximates HugsLib.DefsLoaded entry point
            //LongEventHandler.QueueLongEvent(new Action(() =>
            //{
                // call the code that was previously in DefsLoaded()
            //    if (YayoEndingMod.DebugLogging)
            //    {
            //        Log.Message("[YayoEnding] :: DoDefsLoadedWork");
           //     }
            //    DoDefsLoadedWork();
                // world-dependent code handled in WorldComponent
           // }), "yayoEnding_init", false, null);

            goalBiomeBuffer = settings.goalBiome.ToString();
        }

        public override string SettingsCategory()
        {
            return "yayoEnding_ModName".Translate(); // keep translation key; fallback will show key if missing
        }

        private string goalBiomeBuffer;
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            // --- goalBiome numeric input on a single line ---
            var rect = listing.GetRect(30f);
            var left = rect;
            var right = rect;
            left.width = rect.width * 0.6f;
            right.x = left.xMax;
            right.width = rect.width - left.width;

            Widgets.Label(left, "goalBiome_title".Translate());
            // ensure buffer isn't null
            if (goalBiomeBuffer == null) goalBiomeBuffer = settings.goalBiome.ToString();
            goalBiomeBuffer = Widgets.TextField(right, goalBiomeBuffer);

            // try parse buffer into an int; keep previous value on parse failure
            int parsedGoal;
            int newGoal = settings.goalBiome; // default to current
            if (int.TryParse(goalBiomeBuffer, out parsedGoal))
            {
                // optionally clamp to sensible range; original HugsLib slider used 1..10 so keep that
                newGoal = Mathf.Clamp(parsedGoal, 1, 100); // allow a larger upper bound if wanted
                                                           // keep buffer normalized to parsed value so UI shows cleaned input
                goalBiomeBuffer = newGoal.ToString();
            }

            listing.Gap(6f);

            // --- ignoreExtreme checkbox (needs ref) ---
            // Widgets.CheckboxLabeled signature requires a ref bool
            var rectChk = listing.GetRect(24f);
            Widgets.CheckboxLabeled(rectChk, "yayoEnding_ExcludeExtreme".Translate(), ref settings.ignoreExtreme);
            listing.Gap(6f);

            // --- extract speed slider ---
            listing.Label("extractSpeed_title".Translate() + $": {settings.extractSpeed:F2}");
            float newExtract = Widgets.HorizontalSlider(listing.GetRect(22f), settings.extractSpeed, 0.01f, 50f,
                false, settings.extractSpeed.ToString("F2"));

            listing.Gap(6f);

            // Persist changes if anything changed
            if (newGoal != settings.goalBiome || Math.Abs(newExtract - settings.extractSpeed) > 0.0001f)
            {
                settings.goalBiome = newGoal;
                // keep buffer consistent
                goalBiomeBuffer = settings.goalBiome.ToString();

                settings.extractSpeed = newExtract;
                // apply the same logic as old SettingsChanged()
                ApplySettingsToStatics();
                WriteSettings();
            }

            // We saved the checkbox immediately via ref to settings.ignoreExtreme, but it still needs to be applied:
            // If checkbox changed, ApplySettingsToStatics + WriteSettings to persist that change immediately.
            // To avoid re-applying unnecessarily, compare the static value:
            if (ignoreExtreme != settings.ignoreExtreme)
            {
                ApplySettingsToStatics();
                WriteSettings();
            }

            listing.End();
        }

        private void ApplySettingsToStatics()
        {
            goalBiome = settings.goalBiome;
            ignoreExtreme = settings.ignoreExtreme;
            ExtractSpeed = Mathf.Clamp(settings.extractSpeed, 0.01f, 50f);
        }

        // This replicates the behaviour of DefsLoaded (graphics fix)
        // The defs are created (and this is run) in the PreResolve patch
        public void UpdateGraphics()
        {
            // Update ThingDef graphics for yy_gem_*
            int a = 0;
            foreach (var thing in from t in DefDatabase<ThingDef>.AllDefs
                                  where t.defName.Contains("yy_gem_")
                                  select t)
            {
                var gd = new GraphicData
                {
                    graphicClass = typeof(Graphic_Single),
                    texPath = $"yy_bep{a % 15}"
                };
                thing.graphicData = gd;
                a++;
            }

        }

        //Added logging, but it's not really needed as a mod option
        //Helped in the HugsLib conversion
        public const bool DebugLogging = false;
        

        //added blacklist for biomes ... could be expanded into a mod setting
        //but this works for now
        public static readonly HashSet<string> BlacklistedBiomeNames = new();

        // The combined implementation of patchDef() and patchDef2()
        public void PatchDef()
        {
            if (DebugLogging) Log.Message("[YayoEnding] :: PatchDefs START");
            if (DebugLogging) Log.Message("[YayoEnding] :: PatchDef1 START");
            if (DebugLogging) Log.Message("# generate biome energy item");

            // blacklist some biomes
            BlacklistedBiomeNames.Add("Undercave");            
            BlacklistedBiomeNames.Add("Labyrinth");
            BlacklistedBiomeNames.Add("MetalHell");
            BlacklistedBiomeNames.Add("Underground");
            BlacklistedBiomeNames.Add("Orbit");
            BlacklistedBiomeNames.Add("Space");
            BlacklistedBiomeNames.Add("Sandbar"); //More Vanilla Biomes


            if (DebugLogging)
            {
                Log.Message($"# Blacklist contains {BlacklistedBiomeNames.Count} biome names:");
                foreach (var name in BlacklistedBiomeNames)
                    Log.Message($"    - {name}");
            }

            int countGenerated = 0;

            // generate implied ThingDefs for each biome (respecting ignoreExtreme)
            foreach (var b in DefDatabase<BiomeDef>.AllDefs.Where(biome =>
                    !biome.impassable
                    && (ignoreExtreme || !biome.isExtremeBiome)
                    && !BlacklistedBiomeNames.Contains(biome.defName)))
            {
                if (DebugLogging)
                    Log.Message($"# Including biome: {b.defName} ({b.label})");

                var t = new ThingDef
                {
                    thingClass = typeof(ThingWithComps),
                    category = ThingCategory.Item,
                    resourceReadoutPriority = ResourceCountPriority.Middle,
                    selectable = true,
                    altitudeLayer = AltitudeLayer.Item,
                    comps = new List<CompProperties> { new CompProperties_Forbiddable() },
                    alwaysHaulable = true,
                    drawGUIOverlay = true,
                    rotatable = false,
                    pathCost = 14,

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
                    healthAffectsPrice = false
                };

                // statBases from silver
                t.statBases = new List<StatModifier>(RimWorld.ThingDefOf.Silver.statBases);
                t.thingCategories = new List<ThingCategoryDef>();
                t.stackLimit = 100;
                t.burnableByRecipe = false;
                t.smeltable = false;
                t.terrainAffordanceNeeded = TerrainAffordanceDefOf.Medium;

                // ensure custom category
                t.thingCategories.Add(ThingCategoryDef.Named("yy_gem_piece_category"));
                t.tradeability = Tradeability.None;
                t.tradeTags = new List<string> { "yy_gem" };

                // save def name and register
                arGemDef.Add(t.defName);
                DefGenerator.AddImpliedDef(t);

                countGenerated++;

                if (DebugLogging)
                    Log.Message($"#   -> Generated gem ThingDef: {t.defName}");
            }

            if (DebugLogging)
                Log.Message($"# Total gem defs generated: {countGenerated}");

            if (DebugLogging) Log.Message("[YayoEnding] :: PatchDef1 END");

            PatchDef2();

            if (DebugLogging) Log.Message("[YayoEnding] :: PatchDefs END");
        }


        private void PatchDef2()
        {
            if (DebugLogging) Log.Message("[YayoEnding] :: PatchDef2 START");
            if (DebugLogging) Log.Message("# generate planet energy core recipes");

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
                    workAmount = 1500,
                    recipeUsers = new List<ThingDef>
                {
                    ThingDef.Named("CraftingSpot"),
                    ThingDef.Named("FueledSmithy"),
                    ThingDef.Named("ElectricSmithy"),
                    ThingDef.Named("TableMachining"),
                    ThingDef.Named("FabricationBench")
                },
                    unfinishedThingDef = ThingDef.Named("UnfinishedComponent")
                };

                if (DebugLogging)
                    Log.Message($"# Building recipe {r.defName}");

                var ingredient = new List<IngredientCount>();

                // pick random gem defs (but not duplicating)
                while (ingredient.Count < goalBiome && ingredient.Count < arGemDef.Count)
                {
                    var td = ThingDef.Named(arGemDef[Rand.Range(0, arGemDef.Count)]);
                    var already = ingredient.Any(ic => ic.filter.AllowedThingDefs.Contains(td));
                    if (already) continue;

                    var ing = new IngredientCount();
                    ing.filter.SetAllow(td, true);
                    ing.SetBaseCount(100);
                    ingredient.Add(ing);
                }

                r.ingredients = ingredient;

                if (DebugLogging)
                {
                    Log.Message($"#   Recipe {r.defName} ingredients:");
                    foreach (var ing in ingredient)
                    {
                        foreach (var d in ing.filter.AllowedThingDefs)
                            Log.Message($"      - {d.defName}");
                    }
                }

                r.products = new List<ThingDefCountClass>
            {
                new ThingDefCountClass
                {
                    thingDef = ThingDef.Named("yy_planetCore"),
                    count = 1
                }
            };

                r.skillRequirements = new List<SkillRequirement>
            {
                new SkillRequirement
                {
                    skill = SkillDefOf.Crafting,
                    minLevel = 8
                }
            };

                r.workSkill = SkillDefOf.Crafting;

                DefGenerator.AddImpliedDef(r);

                if (DebugLogging)
                    Log.Message($"#   -> Added recipe def: {r.defName}");
            }

            if (DebugLogging) Log.Message("[YayoEnding] :: PatchDef2 END");
        }
    }
}
