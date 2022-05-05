using System;
using HarmonyLib;
using JetBrains.Annotations;
using KMod;

namespace AnyStartingDupe;

[UsedImplicitly]
public class AnyStartingDupeInfo : UserMod2
{
}

// Debug code to list the personalities from the Db
// IMPORTANT: If the patch below is enabled, then all of the personalities are starting personalities, so this doesn't help much
/*
[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
public static class DebugDbInit
{
	[UsedImplicitly]
	public static void Postfix(Db __instance)
	{
		var personalities = __instance.Personalities;
		Debug.Log("Valid starting personalities:");
		foreach (var startingPersonality in personalities.GetStartingPersonalities())
		{
			Console.WriteLine($"\t{startingPersonality.nameStringKey}");
		}

		Debug.Log("All personalities:");
		foreach (var personalitiesResource in personalities.resources)
		{
			Console.WriteLine($"\t{personalitiesResource.nameStringKey}");
		}
	}
}
*/

[HarmonyPatch(
	typeof(Personality),
	MethodType.Constructor,
	typeof(string),
	typeof(string),
	typeof(string),
	typeof(string),
	typeof(string),
	typeof(string),
	typeof(string),
	typeof(string),
	typeof(int),
	typeof(int),
	typeof(int),
	typeof(int),
	typeof(int),
	typeof(int),
	typeof(string),
	typeof(bool)
)]
public class Personality_Ctor_Patch
{
	[UsedImplicitly]
	public static void Prefix(ref bool isStartingMinion)
	{
		isStartingMinion = true;
	}
}
