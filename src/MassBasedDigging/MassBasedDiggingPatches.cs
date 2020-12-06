using Harmony;
using UnityEngine;

namespace MassBasedDigging
{
    [HarmonyPatch(typeof(Workable), nameof(Workable.GetEfficiencyMultiplier))]
    public static class Workable_GetEfficiencyMultiplier_Patch
    {
        public static void Postfix(ref Workable __instance, ref float __result)
        {
            if(__instance is Diggable)
            {
                __result *= Mathf.Clamp(1200f / Grid.Mass[Grid.PosToCell(__instance)], 0.25f, 10f);
            }
        }
    }
}
