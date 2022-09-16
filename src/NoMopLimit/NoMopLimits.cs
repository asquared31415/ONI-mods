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
	public static readonly MethodInfo CellBelowInfo = AccessTools.Method(typeof(Grid), "CellBelow");

	[UsedImplicitly]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
	{
		var codes = orig.ToList();
		var idx = codes.FindIndex(ci => ci.operand is MethodInfo i && (i == CellBelowInfo));
		if (idx != -1)
		{
			var i = idx - 2;
			// Remove all before store to flag
			codes.RemoveRange(i, 4);
			codes.Insert(i, new CodeInstruction(OpCodes.Ldc_I4_1));
		}

		return codes;
	}
}