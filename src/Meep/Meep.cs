using System;
using System.Linq;
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
	private const string MeepId = "MEEP";

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

		// as of recent updates, the game does not hard code expected lengths, which makes this much simpler
		// as of more recent updates (hot shots), personalities are no longer saved with the dupe, so we can't just remove them
		// instead, they need to be disabled so that they can't be chosen by anything
		foreach (var personality in __instance.resources.Where(personality => personality.Id != MeepId))
		{
			personality.Disabled = true;
		}
	}
}
