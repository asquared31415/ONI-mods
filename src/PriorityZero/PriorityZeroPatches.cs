using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using KMod;
using SquareLib;
using STRINGS;
using UnityEngine;

namespace PriorityZero
{
	public class PriorityZeroInfo : UserMod2
	{
	}

	public class PriorityZero
	{
		private const string ZeroPriority = "PriorityZero.zeroPriority.png";
		private const string ZeroTool = "PriorityZero.zeroTool.png";
		private const string PriorityRendererAtlas = "PriorityZero.priority_overlay_atlas_zero.png";
		public const PriorityScreen.PriorityClass PriorityZeroClass = (PriorityScreen.PriorityClass) (-2);
		public const int PriorityZeroValue = -200;

		public static readonly Sprite ZeroPrioritySprite = ModAssets.AddSpriteFromManifest(ZeroPriority);
		public static readonly Texture2D ZeroToolTexture = ModAssets.LoadTextureFromManifest(ZeroTool);

		public static readonly Texture2D
			PriorityAtlasTexture = ModAssets.LoadTextureFromManifest(PriorityRendererAtlas);
	}

	[HarmonyPatch(
		typeof(Chore),
		MethodType.Constructor,
		typeof(ChoreType),
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
	public class Chore_Ctor_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> origCode)
		{
			var codes = origCode.ToList();
			foreach (var c in codes)
			{
				if (c.opcode == OpCodes.Blt)
				{
					c.operand = 0;
					return codes;
				}
			}

			Debug.LogWarning("[PriorityZero] Unable to find Chore patch offset.");
			return codes;
		}
	}

	[HarmonyPatch(typeof(MinionTodoChoreEntry), nameof(MinionTodoChoreEntry.Apply))]
	public static class MinionTodoChoreEntry_Apply_Patch
	{
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
		private static readonly JobsTableScreen.PriorityInfo PriorityZeroInfo = new JobsTableScreen.PriorityInfo(
			(int) PriorityZero.PriorityZeroClass,
			PriorityZero.ZeroPrioritySprite,
			"Priority Zero"
		);

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

	[HarmonyPatch(typeof(PrioritizeTool), "OnPrefabInit")]
	public static class PrioritizeTool_OnPrefabInit_Patch
	{
		public static void Postfix(PrioritizeTool __instance)
		{
			var newCursors = __instance.cursors.ToList();
			newCursors.Insert(0, PriorityZero.ZeroToolTexture);
			__instance.cursors = newCursors.ToArray();
		}
	}

	[HarmonyPatch(typeof(PrioritizeTool), nameof(PrioritizeTool.Update))]
	public static class PrioritizeTool_Update_Patch
	{
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

	[HarmonyPatch(typeof(PriorityScreen), nameof(PriorityScreen.SetScreenPriority))]
	public static class PriorityScreen_SetScreenPriority_Patches
	{
		// TODO: Clean up
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> origCode)
		{
			var codes = origCode.ToList();
			var buttonsBasic = AccessTools.Field(typeof(PriorityScreen), "buttons_basic");
			var setPriority = AccessTools.Property(typeof(PriorityButton), nameof(PriorityButton.priority))
			                             .GetSetMethod();

			var setSimpleTooltip = AccessTools.Method(typeof(ToolTip), nameof(ToolTip.SetSimpleTooltip));
			var labels = codes[28].labels;
			var buttonsIdx1 = codes.FindIndex(ci => ci.operand is FieldInfo f && f == buttonsBasic);
			var priIdx = codes.FindIndex(ci => ci.operand is MethodInfo m && m == setPriority);
			if (buttonsIdx1 != -1 && priIdx != -1)
			{
				codes.RemoveRange(buttonsIdx1 - 1, priIdx - buttonsIdx1 + 2);
			}
			else
			{
				Debug.LogWarning("[PriorityZero] Unable to patch SetScreenPriority");
				return codes;
			}

			var buttonsIdx2 = codes.FindIndex(ci => ci.operand is FieldInfo f && f == buttonsBasic);
			var tooltipIdx = codes.FindIndex(ci => ci.operand is MethodInfo m && m == setSimpleTooltip);
			if (buttonsIdx2 != -1 && tooltipIdx != -1)
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

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
		{
			var codes = orig.ToList();

			for (var i = 0; i < codes.Count; i++)
			{
				var code = codes[i];
				if (code.opcode == OpCodes.Ldc_R4 && code.operand is 0.1f)
				{
					code.operand = 1f / 11f;
				}

				if (code.opcode == OpCodes.Ldfld && code.operand is FieldInfo info && info == PriorityValue)
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
}
