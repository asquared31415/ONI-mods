using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using KMod;

namespace NoMopLimit;

[UsedImplicitly]
public class NoMopLimitInfo : UserMod2
{
	public override void OnLoad(Harmony harmony)
	{
		MopTool.maxMopAmt = float.PositiveInfinity;
		base.OnLoad(harmony);
	}
}

[HarmonyPatch(typeof(MopTool), "OnDragTool")]
public static class MopTool_OnDragTool_Patch
{
	private static readonly MethodInfo CellBelowInfo = AccessTools.Method(typeof(Grid), "CellBelow");

	private static readonly MethodInfo SolidIndexer =
		AccessTools.DeclaredMethod(typeof(Grid.BuildFlagsSolidIndexer), "get_Item");

	[UsedImplicitly]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
	{
		var codes = orig.ToList();
		var belowIdx = codes.FindIndex(ci => ci.operand is MethodInfo i && (i == CellBelowInfo));
		if (belowIdx == -1)
		{
			Debug.LogWarning("[NoMopLimits] Unable to find Grid.CellBelow");
			return codes;
		}

		var idx = codes.FindIndex(belowIdx, ci => ci.operand is MethodInfo i && i == SolidIndexer);
		if (idx == -1)
		{
			Debug.LogWarning("[NoMopLimits] Unable to find Grid.Solid[]");
			return codes;
		}

		// replace the bool with a false
		codes.Insert(idx + 1, new CodeInstruction(OpCodes.Pop));
		codes.Insert(idx + 2, new CodeInstruction(OpCodes.Ldc_I4_1));
		return codes;
	}
}
