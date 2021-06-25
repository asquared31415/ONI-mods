using HarmonyLib;
using KMod;

namespace UnblockableRockets
{
	public class UnblockableRocketsInfo : UserMod2
	{
	}

	[HarmonyPatch(typeof(ConditionFlightPathIsClear), "CanReachSpace")]
	public class ConditionFlightPathIsClear_CanReachSpace_Patch
	{
		public static bool Prefix(ref bool __result, out int obstruction)
		{
			obstruction = -1;
			__result = true;
			return false;
		}
	}
}
