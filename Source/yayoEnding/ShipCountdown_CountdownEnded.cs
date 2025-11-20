using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoEnding;

[HarmonyPatch(typeof(ShipCountdown), "CountdownEnded")]
internal class ShipCountdown_CountdownEnded
{
    private static readonly FieldInfo fShipRoot = AccessTools.Field(typeof(ShipCountdown), "shipRoot");

    private static readonly AccessTools.FieldRef<Building> s_shipRoot =
        AccessTools.StaticFieldRefAccess<Building>(fShipRoot);

    public static bool Prefix()
    {
        var shipRoot = s_shipRoot.Invoke();

        if(shipRoot == null || shipRoot.def.defName != "yy_teleporter")
        {
            return true;
        }

        var list = new List<Building>();
        var list2 = new List<Pawn>();
        list.Add(shipRoot);
        var stringBuilder = new StringBuilder();
        foreach(var p in shipRoot.Map.mapPawns.FreeColonists)
        {
            stringBuilder.AppendLine($"   {p.LabelCap}");
            ++Find.StoryWatcher.statsRecord.colonistsLaunched;
            TaleRecorder.RecordTale(TaleDefOf.LaunchedShip, p);
            list2.Add(p);
        }

        foreach(var p in list2)
        {
            p.Destroy();
        }

        GameVictoryUtility.ShowCredits(
            GameVictoryUtility.MakeEndCredits(
                "yayoEnding_intro".Translate(),
                "yayoEnding_ending".Translate(),
                stringBuilder.ToString()),
            SongDefOf.EndCreditsSong);
        foreach(var thing in list)
        {
            thing.Destroy();
        }


        return false;
    }
}