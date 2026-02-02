using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace yayoEnding;

public class CompGemMaker : ThingComp
{
    private const float WorkPerPortionBase = 2000f * 60f;
    private int lastUsedTick = -99999;
    private float portionProgress;
    private float portionYieldPct;
    private CompPowerTrader powerComp;

    private ThingDef gemDef => DefDatabase<ThingDef>.GetNamedSilentFail($"yy_gem_{parent.Map.Biome.defName}");

    [Obsolete("Use WorkPerPortionBase constant directly.")]
    public static float WorkPerPortionCurrentDifficulty => WorkPerPortionBase;

    public float ProgressToNextPortionPercent => portionProgress / WorkPerPortionBase;

    public override void PostSpawnSetup(bool respawningAfterLoad) { powerComp = parent.TryGetComp<CompPowerTrader>(); }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref portionProgress, "portionProgress");
        Scribe_Values.Look(ref portionYieldPct, "portionYieldPct");
        Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
    }

    public void DrillWorkDone(Pawn driller, int delta)
    {
        var statValue = driller.GetStatValue(StatDefOf.DeepDrillingSpeed) * YayoEndingMod.ExtractSpeed * delta;
        portionProgress += statValue;
        portionYieldPct += statValue * driller.GetStatValue(StatDefOf.MiningYield) / WorkPerPortionBase;
        lastUsedTick = Find.TickManager.TicksGame;
        if(portionProgress <= WorkPerPortionBase)
        {
            return;
        }

        tryProducePortion();
        portionProgress = 0.0f;
        portionYieldPct = 0.0f;
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        portionProgress = 0.0f;
        portionYieldPct = 0.0f;
        lastUsedTick = -99999;
    }

    private void tryProducePortion()
    {
        var m = parent.Map;
        var thing = ThingMaker.MakeThing(gemDef);
        thing.stackCount = Rand.Range(3, 9);
        GenPlace.TryPlaceThing(thing, parent.InteractionCell, m, ThingPlaceMode.Near);
    }

    public bool CanDrillNow() { return powerComp is not { PowerOn: false }; }

    public bool UsedLastTick() { return lastUsedTick >= Find.TickManager.TicksGame - 1; }

    public override string CompInspectStringExtra()
    {
        if(!parent.Spawned)
        {
            return null;
        }

        return "yayoEnding_resource".Translate() +
            ": " +
            gemDef.label +
            "\n" +
            "ProgressToNextPortion".Translate() +
            ": " +
            ProgressToNextPortionPercent.ToStringPercent("F0");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if(DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "God Mode: Complete Extraction",
                defaultDesc = "Instantly completes the current gem extraction cycle.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower"), // any icon works
                action =
                    () =>
                    {
                        portionProgress = WorkPerPortionBase;
                        portionYieldPct = 1f;

                        tryProducePortion();

                        portionProgress = 0f;
                        portionYieldPct = 0f;

                        Messages.Message("Extraction completed (God Mode).", parent, MessageTypeDefOf.TaskCompletion);
                    }
            };
        }
    }
}