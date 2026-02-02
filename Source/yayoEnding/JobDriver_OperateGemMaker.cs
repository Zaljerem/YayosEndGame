using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace yayoEnding;

public class JobDriver_OperateGemMaker : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var operateGemMaker = this;
        operateGemMaker.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        operateGemMaker.FailOnBurningImmobile(TargetIndex.A);
        operateGemMaker.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Uninstall);
        operateGemMaker.FailOn(() => !job.targetA.Thing.TryGetComp<CompGemMaker>().CanDrillNow());
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        var work = new Toil();
        work.tickIntervalAction = delta =>
        {
            var actor = work.actor;
            ((ThingWithComps)actor.CurJob.targetA.Thing).GetComp<CompGemMaker>().DrillWorkDone(actor, delta);
            actor.skills?.Learn(SkillDefOf.Mining, 0.065f * delta);
        };
        work.defaultCompleteMode = ToilCompleteMode.Never;
        work.WithEffect(EffecterDefOf.DisabledByEMP, TargetIndex.A);
        work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        work.activeSkill = () => SkillDefOf.Mining;
        yield return work;
    }
}