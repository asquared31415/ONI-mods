using HarmonyLib;
using JetBrains.Annotations;
using KMod;

namespace MovableGraves;

[UsedImplicitly]
public class MovableGravesInfo : UserMod2 {
    public override void OnLoad(Harmony harmony) {
        base.OnLoad(harmony);

        LocString.CreateLocStringKeys(typeof(MovableGravesStrings.STRINGS), null);
    }
}
