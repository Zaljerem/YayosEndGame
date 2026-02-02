using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoEnding;

public class YayoEndingMod : Mod
{
    private static readonly List<string> arGemDef = [];


    //added blacklist for biomes ... could be expanded into a mod setting
    //but this works for now
    private static readonly HashSet<string> BlacklistedBiomeNames = [];
    public static float ExtractSpeed = 1f;
    public static int goalBiome = 2;
    private static bool ignoreExtreme;

    private static YayoEndingMod Instance;

    private readonly YayoEndingSettings settings;

    private string goalBiomeBuffer;

    public YayoEndingMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<YayoEndingSettings>();

        Instance = this;
        importOldHugsLibSettings();

        // apply persisted settings into static fields
        ApplySettingsToStatics();

        DebugLogging("[YayoEnding] :: Harmony patching");

        new Harmony("yayoEnding").PatchAll();

        goalBiomeBuffer = settings.goalBiome.ToString();
    }

    private static void importOldHugsLibSettings()
    {
        var hugsLibConfig = Path.Combine(GenFilePaths.SaveDataFolderPath, "HugsLib", "ModSettings.xml");
        if (!new FileInfo(hugsLibConfig).Exists)
        {
            return;
        }

        var xml = XDocument.Load(hugsLibConfig);
        var modNodeName = "yayoEnding";

        var modSettings = xml.Root?.Element(modNodeName);
        if (modSettings == null)
        {
            return;
        }

        foreach (var modSetting in modSettings.Elements())
        {
            if (modSetting.Name == "goalBiome")
            {
                Instance.settings.goalBiome = int.Parse(modSetting.Value);
            }

            if (modSetting.Name == "ignoreExtreme")
            {
                Instance.settings.ignoreExtreme = bool.Parse(modSetting.Value);
            }

            if (modSetting.Name == "extractSpeed")
            {
                Instance.settings.extractSpeed = float.Parse(modSetting.Value);
            }
        }

        Instance.settings.Write();
        xml.Root.Element(modNodeName)?.Remove();
        xml.Save(hugsLibConfig);

        Log.Message("[YayoCombat3]: Imported old HugLib-settings");
    }

    private void ApplySettingsToStatics()
    {
        goalBiome = settings.goalBiome;
        ignoreExtreme = settings.ignoreExtreme;
        ExtractSpeed = Mathf.Clamp(settings.extractSpeed, 0.01f, 50f);
    }


    private static void PatchDef2()
    {
        DebugLogging("[YayoEnding] :: PatchDef2 START");
        DebugLogging("# generate planet energy core recipes");

        for (var i = 0; i < 4; i++)
        {
            var r = new RecipeDef
            {
                defName = $"Make_yy_planetCore_{i + 1}",
                label =
                    string.Format(
                        "yayoEnding_energyCore_recipe_label".Translate(),
                        ThingDef.Named("yy_planetCore").label,
                        (i + 1).ToString()),
                description = ThingDef.Named("yy_planetCore").description,
                jobString =
                    string.Format(
                        "yayoEnding_energyCore_recipe_jobstring".Translate(),
                        ThingDef.Named("yy_planetCore").label),
                workSpeedStat = StatDefOf.GeneralLaborSpeed,
                effectWorking = EffecterDefOf.Drill,
                soundWorking = SoundDef.Named("Recipe_Machining"),
                workAmount = 1500,
                recipeUsers =
                [
                    ThingDef.Named("CraftingSpot"),
                    ThingDef.Named("FueledSmithy"),
                    ThingDef.Named("ElectricSmithy"),
                    ThingDef.Named("TableMachining"),
                    ThingDef.Named("FabricationBench")
                ],
                unfinishedThingDef = ThingDef.Named("UnfinishedComponent")
            };

            DebugLogging($"# Building recipe {r.defName}");

            var ingredient = new List<IngredientCount>();

            // pick random gem defs (but not duplicating)
            while (ingredient.Count < goalBiome && ingredient.Count < arGemDef.Count)
            {
                var td = ThingDef.Named(arGemDef[Rand.Range(0, arGemDef.Count)]);
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

            DebugLogging($"#   Recipe {r.defName} ingredients:");
            foreach (var ing in ingredient)
            {
                DebugLogging($"      - {string.Join(", ", ing.filter.AllowedThingDefs)}");
            }


            r.products = [new ThingDefCountClass() { thingDef = ThingDef.Named("yy_planetCore"), count = 1 }];

            r.skillRequirements = [new SkillRequirement() { skill = SkillDefOf.Crafting, minLevel = 8 }];

            r.workSkill = SkillDefOf.Crafting;

            DefGenerator.AddImpliedDef(r);

            DebugLogging($"#   -> Added recipe def: {r.defName}");
        }

        DebugLogging("[YayoEnding] :: PatchDef2 END");
    }

    //Added logging, but it's not really needed as a mod option
    //Helped in the HugsLib conversion
    public static void DebugLogging(string message)
    {
        if (false)
        {
#pragma warning disable CS0162 // Unreachable code detected
            Log.Message($"[YayoEnding] :: {message}");
#pragma warning restore CS0162 // Unreachable code detected
        }
    }

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
        goalBiomeBuffer ??= settings.goalBiome.ToString();

        goalBiomeBuffer = Widgets.TextField(right, goalBiomeBuffer);

        // try parse buffer into an int; keep previous value on parse failure
        var newGoal = settings.goalBiome; // default to current
        if (int.TryParse(goalBiomeBuffer, out var parsedGoal))
        {
            // optionally clamp to sensible range; original HugsLib slider used 1..10 so keep that
            newGoal = Mathf.Clamp(parsedGoal, 1, 100); // allow a larger upper bound if wanted
            // to keep buffer normalized to parsed value so UI shows cleaned input
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
        var newExtract = Widgets.HorizontalSlider(
            listing.GetRect(22f),
            settings.extractSpeed,
            0.01f,
            50f,
            false,
            settings.extractSpeed.ToString("F2"));

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

    // The combined implementation of patchDef() and patchDef2()
    public static void PatchDef()
    {
        DebugLogging("[YayoEnding] :: PatchDefs START");
        DebugLogging("[YayoEnding] :: PatchDef1 START");
        DebugLogging("# generate biome energy item");

        // blacklist some biomes
        BlacklistedBiomeNames.Add("Undercave");
        BlacklistedBiomeNames.Add("Labyrinth");
        BlacklistedBiomeNames.Add("MetalHell");
        BlacklistedBiomeNames.Add("Underground");
        BlacklistedBiomeNames.Add("Orbit");
        BlacklistedBiomeNames.Add("Space");
        BlacklistedBiomeNames.Add("Sandbar"); //More Vanilla Biomes


        DebugLogging(
            $"# Blacklist contains {BlacklistedBiomeNames.Count} biome names: {string.Join(", ", BlacklistedBiomeNames)}");

        var countGenerated = 0;

        // generate implied ThingDefs for each biome (respecting ignoreExtreme)
        foreach (var b in DefDatabase<BiomeDef>.AllDefs
                     .Where(biome => !biome.impassable &&
                                     (ignoreExtreme || !biome.isExtremeBiome) &&
                                     !BlacklistedBiomeNames.Contains(biome.defName)))
        {
            DebugLogging($"# Including biome: {b.defName} ({b.label})");

            var t = new ThingDef
            {
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

                defName = $"yy_gem_{b.defName}",
                label = string.Format("yayoEnding_energyPiece".Translate(), b.label),
                description = string.Format("yayoEnding_energyPiece".Translate(), b.label),

                graphicData =
                    new GraphicData
                        { texPath = "Things/Item/Resource/Gold", graphicClass = typeof(Graphic_StackCount) },

                soundInteract = SoundDef.Named("Silver_Drop"),
                soundDrop = SoundDef.Named("Silver_Drop"),
                useHitPoints = false,
                healthAffectsPrice = false,
                // statBases from silver
                statBases = [..RimWorld.ThingDefOf.Silver.statBases],
                thingCategories = [],
                stackLimit = 100,
                burnableByRecipe = false,
                smeltable = false,
                terrainAffordanceNeeded = TerrainAffordanceDefOf.Medium
            };

            // ensure custom category
            t.thingCategories.Add(ThingCategoryDef.Named("yy_gem_piece_category"));
            t.tradeability = Tradeability.None;
            t.tradeTags = ["yy_gem"];

            // save def name and register
            arGemDef.Add(t.defName);
            DefGenerator.AddImpliedDef(t);

            countGenerated++;

            DebugLogging($"#   -> Generated gem ThingDef: {t.defName}");
        }

        DebugLogging($"# Total gem defs generated: {countGenerated}");

        DebugLogging("[YayoEnding] :: PatchDef1 END");

        PatchDef2();

        DebugLogging("[YayoEnding] :: PatchDefs END");
    }

    public override string SettingsCategory()
    {
        return "yayoEnding_ModName".Translate(); // keep translation key; fallback will show key if missing
    }

    // This replicates the behaviour of DefsLoaded (graphics fix)
    // The defs are created (and this is run) in the PreResolve patch
    public static void UpdateGraphics()
    {
        // Update ThingDef graphics for yy_gem_*
        var a = 0;
        foreach (var thing in from t in DefDatabase<ThingDef>.AllDefs where t.defName.Contains("yy_gem_") select t)
        {
            var gd = new GraphicData { graphicClass = typeof(Graphic_Single), texPath = $"yy_bep{a % 15}" };
            thing.graphicData = gd;
            a++;
        }
    }
}