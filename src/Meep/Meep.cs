using System;
using Database;
using HarmonyLib;
using KMod;

namespace Meep
{
	public class MeepInfo : UserMod2
	{
	}

	[HarmonyPatch(typeof(Personalities), MethodType.Constructor, new Type[0])]
	public class Meep
	{
		public static void Postfix(Personalities __instance)
		{
			var meeps = __instance.resources.Count;
			var meep = __instance.resources.Find(p => p.Id == "Meep");
			for (var i = 0; i < meeps; ++i)
			{
				__instance.resources[i] = meep;
			}
		}
	}
}
