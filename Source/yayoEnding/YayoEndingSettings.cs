using Verse;

namespace yayoEnding;

public class YayoEndingSettings : ModSettings
{
    public float extractSpeed = 1f;
    public int goalBiome = 2;
    public bool ignoreExtreme;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref goalBiome, "goalBiome", 2);
        Scribe_Values.Look(ref extractSpeed, "extractSpeed", 1f);
        Scribe_Values.Look(ref ignoreExtreme, "ignoreExtreme");
    }
}