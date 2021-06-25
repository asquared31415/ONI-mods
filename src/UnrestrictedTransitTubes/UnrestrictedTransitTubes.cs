using Harmony;

namespace UnrestrictedTransitTubes
{
	[HarmonyPatch(typeof(UtilityNetworkTubesManager), "CanAddConnection")]
	public class UtilityNetworkTubesManager_CanAddConnection_Patch
	{
		public static void Postfix(ref bool __result)
		{
			__result = true;
		}
	}
}
