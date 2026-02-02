using RimWorld;
using Verse;
using Verse.AI;

namespace yayoEnding;

public class WorkGiver_GemMaker : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.yy_biomeExtractor);

    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        var buildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
        foreach (var building in buildingsColonist)
        {
            if (building.def != ThingDefOf.yy_biomeExtractor)
            {
                continue;
            }

            var comp = building.GetComp<CompPowerTrader>();
            if ((comp == null || comp.PowerOn) &&
                building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null)
            {
                return false;
            }
        }

        return true;
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return t.Faction == pawn.Faction &&
               t is Building building &&
               !building.IsForbidden(pawn) &&
               pawn.CanReserve((LocalTargetInfo)(Thing)building, ignoreOtherReservations: forced) &&
               building.TryGetComp<CompGemMaker>().CanDrillNow() &&
               building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null &&
               !building.IsBurning();
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(JobDefOf.OperateGemMaker, (LocalTargetInfo)t, 1500, true);
    }
}