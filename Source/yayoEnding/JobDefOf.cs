using RimWorld;
using Verse;

namespace yayoEnding;

[DefOf]
public static class JobDefOf
{
    public static JobDef OperateGemMaker;

    static JobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }
}