using Harmony;

namespace UnblockableRockets
{
	[HarmonyPatch(typeof(ConditionFlightPathIsClear), "CanReachSpace")]
	public class ConditionFlightPathIsClear_CanReachSpace_Patch
	{
		public static bool Prefix(ref bool __result, int ___obstructedTile)
		{
			___obstructedTile = -1;
			__result = true;
			return false;
		}
	}
}
