using HarmonyLib;
using Klei.AI;

namespace BreathTraitsAffectSuffocation
{
	[HarmonyPatch(typeof(SuffocationMonitor.Instance), MethodType.Constructor, typeof(OxygenBreather))]
	public static class BreathTraitPatches
	{
		public static void Postfix(ref SuffocationMonitor.Instance __instance, ref OxygenBreather oxygen_breather)
		{
			var minionIdentity = oxygen_breather.gameObject.GetComponent<MinionIdentity>();
			if (minionIdentity.GetComponent<Traits>().HasTrait("DiversLung"))
			{
				__instance.holdingbreath.SetValue(__instance.holdingbreath.Value * 3f / 4f);
			}

			if (minionIdentity.GetComponent<Traits>().HasTrait("MouthBreather"))
			{
				__instance.holdingbreath.SetValue(__instance.holdingbreath.Value * 2);
			}

			if (minionIdentity.GetComponent<Traits>().HasTrait("DeeperDiversLungs"))
			{
				__instance.holdingbreath.SetValue(__instance.holdingbreath.Value / 2);
			}
		}
	}
}
