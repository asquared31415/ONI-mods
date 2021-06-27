using HarmonyLib;
using KMod;

namespace AnyStartingDupe
{
	public class AnyStartingDupeInfo : UserMod2
	{
	}

	// This is some testing code to get all the dupe names that are normally excluded
	// The constants here come from the code patched below in the random range calls
	/*
	[HarmonyPatch(typeof(Db), "Initialize")]
	public static class GetOtherDupes
	{
		public static void Postfix()
		{
			var db = Db.Get().Personalities;
			for (var i = 0; i < 35; i++)
			{
				Debug.Log($"Dupe name: {db[i].nameStringKey}");
			}
		}
	}
	*/

	[HarmonyPatch(typeof(MinionStartingStats), MethodType.Constructor, typeof(bool), typeof(string))]
	public class AnyStartingDupe
	{
		public static void Prefix(ref bool is_starter_minion)
		{
			is_starter_minion = false;
		}
	}
}
