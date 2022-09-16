using System;
using Database;
using HarmonyLib;
using JetBrains.Annotations;
using KMod;

namespace Meep;

[UsedImplicitly]
public class MeepInfo : UserMod2
{
}

[HarmonyPatch(typeof(Personalities), MethodType.Constructor, new Type[0])]
public static class Meep
{
	private const string MeepId = "Meep";

	[UsedImplicitly]
	public static void Postfix(Personalities __instance)
	{
		var meep = __instance.resources.Find(p => p.Id == MeepId);
		if (meep == null)
		{
			Debug.LogWarning("[Meep] Unable to find Meep, cannot make him the only dupe!");
			return;
		}

		// make Meep a starting minion to make sure that a valid starting minion exists
		meep.startingMinion = true;

		// remove all other dupes
		// as of recent updates, the game does not hard code expected lengths, which makes this much simpler
		__instance.resources.RemoveAll(p => p.Id != MeepId);
	}
}
