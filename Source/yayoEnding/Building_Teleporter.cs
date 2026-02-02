using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoEnding;

public class Building_Teleporter : Building
{
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        foreach (var gizmo2 in ShipUtility.ShipStartupGizmos(this))
        {
            yield return gizmo2;
        }

        var commandAction = new Command_Action
        {
            action = tryLaunch,
            defaultLabel = "yayoEnding_CommandTeleporterLaunch".Translate(),
            defaultDesc = "yayoEnding_CommandTeleporterLaunchDesc".Translate()
        };

        var comp = this.TryGetComp<CompHibernatable>();
        if (comp != null && comp.State == HibernatableStateDefOf.Hibernating || comp is { Running: false })
        {
            commandAction.Disable("yayoEnding_energyChargeRequired".Translate());
        }


        if (ShipCountdown.CountingDown)
        {
            commandAction.Disable();
        }

        commandAction.hotKey = KeyBindingDefOf.Misc1;
        commandAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");
        yield return commandAction;
    }

    private void forceLaunch()
    {
        ShipCountdown.InitiateCountdown(this);
        if (Spawned)
        {
            QuestUtility.SendQuestTargetSignals(Map.Parent.questTags, "LaunchedShip");
        }
    }

    private void tryLaunch()
    {
        forceLaunch();
    }
}