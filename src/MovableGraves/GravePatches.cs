using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace MovableGraves;

public static class GravePatches
{
	[HarmonyPatch(typeof(GraveConfig), nameof(GraveConfig.ConfigureBuildingTemplate))]
	public static class GraveConfig_ConfigureBuildingTemplate_Patch
	{
		[UsedImplicitly]
		public static void Postfix(GameObject go)
		{
			go.AddOrGet<GraveDeconstructDropper>();
		}
	}

	[HarmonyPatch(typeof(Grave), "OnStorageChanged")]
	public static class Grave_StorageChanged_Patch
	{
		[UsedImplicitly]
		public static void Prefix(object data, Grave __instance)
		{
			if (data is not GameObject go)
			{
				return;
			}

			// copy the data over in 1 tick if it's a special info item
			if (go.TryGetComponent<GraveInfoItem>(out var infoItem))
			{
				var graveName = infoItem.graveName;
				var epitaphIdx = infoItem.epitaphIdx;
				var burialTime = infoItem.burialTime;
				GameScheduler.Instance.ScheduleNextFrame(
					"MovableGraves.FixGraveInfo",
					_ =>
					{
						__instance.graveName = graveName;
						__instance.epitaphIdx = epitaphIdx;
						__instance.burialTime = burialTime;
					}
				);
			}
		}
	}

	[HarmonyPatch(typeof(Grave.StatesInstance), nameof(Grave.StatesInstance.CreateFetchTask))]
	public static class Grave_CreateChore_Patch
	{
		private static readonly ConstructorInfo FetchChoreCtor = AccessTools.Constructor(
			typeof(FetchChore),
			new[]
			{
				typeof(ChoreType), typeof(Storage), typeof(float), typeof(HashSet<Tag>),
				typeof(FetchChore.MatchCriteria), typeof(Tag), typeof(Tag[]), typeof(ChoreProvider), typeof(bool),
				typeof(System.Action<Chore>), typeof(System.Action<Chore>), typeof(System.Action<Chore>),
				typeof(Operational.State), typeof(int),
			}
		);

		private static readonly FieldInfo MinionTag = AccessTools.Field(typeof(GameTags), nameof(GameTags.Minion));

		[UsedImplicitly]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
		{
			var codes = orig.ToList();

			var ctorIdx = codes.FindIndex(
				ci => (ci.opcode == OpCodes.Newobj) && ci.operand is ConstructorInfo ctor && (ctor == FetchChoreCtor)
			);
			if (ctorIdx == -1)
			{
				Debug.LogWarning("[Movable Graves] Unable to find Grave fetch chore ctor");
				return codes;
			}

			var minionIdx = codes.FindLastIndex(ctorIdx, ci => ci.LoadsField(MinionTag));
			if (minionIdx == -1)
			{
				Debug.LogWarning("[Movable Graves] Unable to find Minion tag");
				return codes;
			}

			// +1 for HashSet::add, +1 for popping the ret bool, +1 to insert after
			var insertIdx = minionIdx + 3;

			codes.InsertRange(
				insertIdx,
				new[]
				{
					new CodeInstruction(OpCodes.Dup),
					CodeInstruction.LoadField(typeof(GraveInfoItemConfig), nameof(GraveInfoItemConfig.Tag)),
					CodeInstruction.Call(typeof(HashSet<Tag>), nameof(HashSet<Tag>.Add), new[] { typeof(Tag) }),
					new CodeInstruction(OpCodes.Pop),
				}
			);

			return codes;
		}
	}
}
