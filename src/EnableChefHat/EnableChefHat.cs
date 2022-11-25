using HarmonyLib;
using JetBrains.Annotations;
using KMod;

namespace EnableChefHat;

public class EnableChefHat : UserMod2
{
}

[HarmonyPatch(typeof(MinionResume), nameof(MinionResume.IsAbleToLearnSkill))]
public class MinionResume_IsAbleToLearnSkill_Patch
{
	[UsedImplicitly]
	public static void Postfix(ref bool __result, string skillId)
	{
		if (skillId == Db.Get().Skills.Cooking1.Id)
		{
			__result = true;
		}
	}
}