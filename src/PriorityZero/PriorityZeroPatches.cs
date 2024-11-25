using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using KMod;
using SquareLib;
using STRINGS;
using UnityEngine;

namespace PriorityZero;

[UsedImplicitly]
public class PriorityZeroInfo : UserMod2
{
}

public static class PriorityZero
{
	private const string ZeroPriority = "PriorityZero.zeroPriority.png";
	private const string ZeroTool = "PriorityZero.zeroTool.png";
	private const string PriorityRendererAtlas = "PriorityZero.priority_overlay_atlas_zero.png";
	public const PriorityScreen.PriorityClass PriorityZeroClass = (PriorityScreen.PriorityClass) (-2);
	public const int PriorityZeroValue = -200;

	public static readonly Chore.Precondition ZeroPrecondition = new()
	{
		id = nameof(ZeroPrecondition), description = "Priority Zero",
		fn = (ref Chore.Precondition.Context context, object _) =>
			context.chore.masterPriority.priority_class != PriorityZeroClass,
	};

	public static readonly Sprite ZeroPrioritySprite = ModAssets.AddSpriteFromManifest(ZeroPriority);
	public static readonly Texture2D ZeroToolTexture = ModAssets.LoadTextureFromManifest(ZeroTool);

	public static readonly Texture2D PriorityAtlasTexture = ModAssets.LoadTextureFromManifest(PriorityRendererAtlas);
}

[HarmonyPatch(
	typeof(StandardChoreBase),
	MethodType.Constructor,
	typeof(ChoreType),
	typeof(IStateMachineTarget),
	typeof(ChoreProvider),
	typeof(bool),
	typeof(Action<Chore>),
	typeof(Action<Chore>),
	typeof(Action<Chore>),
	typeof(PriorityScreen.PriorityClass),
	typeof(int),
	typeof(bool),
	typeof(bool),
	typeof(int),
	typeof(bool),
	typeof(ReportManager.ReportType)
)]
public static class Chore_Ctor_Patch
{
	private static readonly MethodInfo DebugErrMethod = AccessTools.Method(
		typeof(Debug),
		nameof(Debug.LogErrorFormat),
		new[] { typeof(string), typeof(object[]) }
	);

	[UsedImplicitly]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> origCode)
	{
		var codes = origCode.ToList();

		var foundIdx = -1;

		// Remove the first < 1 branch that errors if a priority is less than 1
		// loop up to the third to last index, since the last one can't be the start of a pair, and there needs
		// to be a block to put the label on after
		for (var idx = 1; idx < codes.Count - 1; idx++)
		{
			if (codes[idx].LoadsConstant(1) && (codes[idx + 1].opcode == OpCodes.Blt_S))
			{
				foundIdx = idx;
				break;
			}
		}

		if (foundIdx == -1)
		{
			Debug.LogError("[Priority Zero] unable to patch Chore ctor, no comparison");
			return codes;
		}

		// Move the label to the block after
		codes[foundIdx + 2].MoveLabelsFrom(codes[foundIdx - 1]);

		// remove the previous load of the priority, then the comparison load and branch
		codes.RemoveRange(foundIdx - 1, 3);

		// remove the label that was used for the <1 case for error reporting
		var errIdx = codes.FindIndex(ci => ci.Calls(DebugErrMethod));
		if (errIdx == -1)
		{
			Debug.LogError($"[Priority Zero] unable to patch StandardChoreBase ctor: no {DebugErrMethod.Name} call found");
			return codes;
		}

		// the label will be before the start of the block which loads the format string
		var formatIdx = codes.FindLastIndex(errIdx, ci => ci.opcode == OpCodes.Ldstr);
		if (formatIdx == -1)
		{
			Debug.LogError("[Priority Zero] unable to patch StandardChoreBase ctor: no format string found");
			return codes;
		}

		var labelIdx = codes.FindLastIndex(formatIdx, ci => ci.labels.Count > 0);
		if (labelIdx == -1)
		{
			Debug.LogError("[Priority Zero] unable to find a label before format string");
			return codes;
		}

		var _ = codes[formatIdx].ExtractLabels();

		return codes;
	}

	[UsedImplicitly]
	public static void Postfix(StandardChoreBase __instance)
	{
		__instance.AddPrecondition(PriorityZero.ZeroPrecondition);
	}
}

[HarmonyPatch(typeof(MinionTodoChoreEntry), nameof(MinionTodoChoreEntry.Apply))]
public static class MinionTodoChoreEntry_Apply_Patch
{
	[UsedImplicitly]
	public static void Postfix(MinionTodoChoreEntry __instance, Chore.Precondition.Context context)
	{
		if (context.chore.masterPriority.priority_class == PriorityZero.PriorityZeroClass)
		{
			__instance.priorityIcon.sprite = PriorityZero.ZeroPrioritySprite;
			__instance.priorityLabel.text = "0";
		}
	}
}

[HarmonyPatch(typeof(MinionTodoSideScreen), nameof(MinionTodoSideScreen.priorityInfo), MethodType.Getter)]
public static class MinionTodoSideScreen_PriorityInfo_Patch
{
	private static readonly JobsTableScreen.PriorityInfo PriorityZeroInfo = new(
		(int) PriorityZero.PriorityZeroClass,
		PriorityZero.ZeroPrioritySprite,
		"Priority Zero"
	);

	[UsedImplicitly]
	public static void Postfix(MinionTodoSideScreen __instance, List<JobsTableScreen.PriorityInfo> __result)
	{
		if (__result.Contains(PriorityZeroInfo))
		{
			return;
		}

		__result.Add(PriorityZeroInfo);
		Traverse.Create(__instance).Field("_priorityInfo").SetValue(__result);
	}
}

// Add zero cursor to priority cursors list
[HarmonyPatch(typeof(PrioritizeTool), "OnPrefabInit")]
public static class PrioritizeTool_OnPrefabInit_Patch
{
	[UsedImplicitly]
	public static void Postfix(PrioritizeTool __instance)
	{
		var newCursors = __instance.cursors.ToList();
		newCursors.Insert(0, PriorityZero.ZeroToolTexture);
		__instance.cursors = newCursors.ToArray();
	}
}

// Handle cursor when priority is selected
[HarmonyPatch(typeof(PrioritizeTool), nameof(PrioritizeTool.Update))]
public static class PrioritizeTool_Update_Patch
{
	[UsedImplicitly]
	public static void Prefix()
	{
		var priority = Traverse.Create(ToolMenu.Instance.PriorityScreen).Field("lastSelectedPriority");
		var value = (PrioritySetting) priority.GetValue();
		if (value.priority_class == PriorityZero.PriorityZeroClass)
		{
			value.priority_value = 1;
		}
		else
		{
			value.priority_value += 1;
		}

		priority.SetValue(value);
	}

	[UsedImplicitly]
	public static void Postfix()
	{
		var priority = Traverse.Create(ToolMenu.Instance.PriorityScreen).Field("lastSelectedPriority");
		var value = (PrioritySetting) priority.GetValue();
		if (value.priority_class == PriorityZero.PriorityZeroClass)
		{
			value.priority_value = PriorityZero.PriorityZeroValue;
		}
		else
		{
			value.priority_value -= 1;
		}

		priority.SetValue(value);
	}
}

[HarmonyPatch(typeof(PriorityScreen), nameof(PriorityScreen.InstantiateButtons))]
public static class PriorityScreen_InstantiateButtons_Patch
{
	[UsedImplicitly]
	public static void Prefix(PriorityScreen __instance, Action<PrioritySetting> on_click, bool playSelectionSound)
	{
		var buttonPrefab = (PriorityButton) Traverse.Create(__instance).Field("buttonPrefab_basic").GetValue();
		var priorityButton = Util.KInstantiateUI<PriorityButton>(
			buttonPrefab.gameObject,
			buttonPrefab.transform.parent.gameObject
		);

		priorityButton.playSelectionSound = playSelectionSound;
		priorityButton.onClick = on_click;
		priorityButton.text.text = "0";
		priorityButton.priority = new PrioritySetting(
			PriorityZero.PriorityZeroClass,
			PriorityZero.PriorityZeroValue
		);

		priorityButton.tooltip.SetSimpleTooltip(string.Format(UI.PRIORITYSCREEN.BASIC, 0));

		var buttonsField = Traverse.Create(__instance).Field("buttons_basic");
		var buttons = (List<PriorityButton>) buttonsField.GetValue();
		buttons.Insert(0, priorityButton);
		buttonsField.SetValue(buttons);
	}
}

// Make the buttons on the bar match up
[HarmonyPatch(typeof(PriorityScreen), nameof(PriorityScreen.SetScreenPriority))]
public static class PriorityScreen_SetScreenPriority_Patches
{
	[UsedImplicitly]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> origCode)
	{
		var codes = origCode.ToList();
		var buttonsBasic = AccessTools.Field(typeof(PriorityScreen), "buttons_basic");
		var setPriority = AccessTools.Property(typeof(PriorityButton), nameof(PriorityButton.priority))
			.GetSetMethod();

		var setSimpleTooltip = AccessTools.Method(typeof(ToolTip), nameof(ToolTip.SetSimpleTooltip));
		var labels = codes[28].labels;
		var buttonsIdx1 = codes.FindIndex(ci => ci.operand is FieldInfo f && (f == buttonsBasic));
		var priIdx = codes.FindIndex(ci => ci.operand is MethodInfo m && (m == setPriority));
		if ((buttonsIdx1 != -1) && (priIdx != -1))
		{
			codes.RemoveRange(buttonsIdx1 - 1, priIdx - buttonsIdx1 + 2);
		}
		else
		{
			Debug.LogWarning("[PriorityZero] Unable to patch SetScreenPriority");
			return codes;
		}

		var buttonsIdx2 = codes.FindIndex(ci => ci.operand is FieldInfo f && (f == buttonsBasic));
		var tooltipIdx = codes.FindIndex(ci => ci.operand is MethodInfo m && (m == setSimpleTooltip));
		if ((buttonsIdx2 != -1) && (tooltipIdx != -1))
		{
			codes[tooltipIdx + 1].labels = labels;
			codes.RemoveRange(buttonsIdx2 - 1, tooltipIdx - buttonsIdx2 + 2);
		}
		else
		{
			Debug.LogWarning("[PriorityZero] Unable to patch SetScreenPriority");
			return codes;
		}

		return codes;
	}
}

// Change the atlas texture for the priority overlay to be custom and have a 0 icon
[HarmonyPatch(typeof(PrioritizableRenderer), MethodType.Constructor)]
public static class PrioritizableRenderer_ctor_Patch
{
	[UsedImplicitly]
	public static void Postfix(Material ___material)
	{
		var texture = PriorityZero.PriorityAtlasTexture;
		___material.SetTexture(Shader.PropertyToID("_MainTex"), texture);
	}
}

// Helper class to apply the above changes to the atlas
[HarmonyPatch(typeof(PrioritizableRenderer), nameof(PrioritizableRenderer.RenderEveryTick))]
public static class PrioritizableRenderer_RenderEveryTick_Patch
{
	private static readonly FieldInfo PriorityValue = AccessTools.Field(
		typeof(PrioritySetting),
		nameof(PrioritySetting.priority_value)
	);

	private static readonly MethodInfo AdjustedVal = AccessTools.Method(
		typeof(PrioritizableRenderer_RenderEveryTick_Patch),
		nameof(AdjustedValue)
	);

	[UsedImplicitly]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
	{
		var codes = orig.ToList();

		for (var i = 0; i < codes.Count; i++)
		{
			var code = codes[i];
			if ((code.opcode == OpCodes.Ldc_R4) && code.operand is 0.1f)
			{
				code.operand = 1f / 11f;
			}

			if ((code.opcode == OpCodes.Ldfld) && code.operand is FieldInfo info && (info == PriorityValue))
			{
				codes[i] = new CodeInstruction(OpCodes.Call, AdjustedVal);
			}
		}

		return codes;
	}

	private static float AdjustedValue(PrioritySetting setting)
	{
		if (setting.priority_class == PriorityZero.PriorityZeroClass)
		{
			return 1;
		}

		return setting.priority_value + 1;
	}
}
