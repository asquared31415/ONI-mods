using HarmonyLib;
using KMod;

namespace TemporalTearOpener
{
	public class TemporalTearOpenerInfo : UserMod2
	{
	}

	[HarmonyPatch(typeof(TemporalTear), "OnSpawn")]
	public static class TemporalTear_OnSpawn_Patch
	{
		public static void Prefix(ref bool ___m_open)
		{
			___m_open = true;
		}
	}
}
