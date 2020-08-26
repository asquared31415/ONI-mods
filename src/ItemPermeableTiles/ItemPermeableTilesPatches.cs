using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CaiLib.Utils;
using Harmony;
using UnityEngine;

namespace ItemPermeableTiles
{
	public static class ItemPermeableTilesOnLoad
	{
		public static void OnLoad()
		{
			BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, ItemPermeableTileConfig.ID);
			LocString.CreateLocStringKeys(typeof(STRINGS), null);
		}
	}

	[HarmonyPatch(typeof(GravityComponents), "FixedUpdate")]
	public static class GravityComponents_FixedUpdate_Patch
	{
		private static readonly MethodInfo IsPermeable = AccessTools.Method(
			typeof(GravityComponents_FixedUpdate_Patch),
			nameof(IsCellPermeable)
		);

		private static readonly FieldInfo Transform = AccessTools.Field(typeof(GravityComponent), "transform");
		private static readonly FieldInfo X = AccessTools.Field(typeof(Vector2), "x");
		private static readonly FieldInfo Y = AccessTools.Field(typeof(Vector2), "y");
		private static readonly FieldInfo Z = AccessTools.Field(typeof(Vector3), "z");

		private static readonly ConstructorInfo Vec3 = AccessTools.Constructor(
			typeof(Vector3),
			new[] {typeof(float), typeof(float), typeof(float)}
		);

		private static readonly MethodInfo SetTransform = AccessTools.Method(
			typeof(TransformExtensions),
			"SetPosition"
		);

		private static readonly FieldInfo Data = AccessTools.Field(typeof(GravityComponents), "data");

		private static readonly MethodInfo ListSetIndex = AccessTools.Method(
			typeof(List<GravityComponent>),
			"set_Item"
		);

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig, ILGenerator gen)
		{
			List<CodeInstruction> codes = orig.ToList();
			for(var i = 0; i < codes.Count; ++i)
			{
				if(codes[i].operand is 1188683690)
				{
					var idx = codes.FindLastIndex(i, ci => ci.opcode == OpCodes.Brfalse) - 1;
					// idx is on the load
					// Steal labels on the if(flag3)
					// ToList clones the labels
					var labels = codes[idx].labels.ToList();
					codes[idx].labels.Clear();
					// Load cell and pass to helper
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_3));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_S, (byte)10) {labels = labels});
					codes.Insert(idx++, new CodeInstruction(OpCodes.Call, IsPermeable));

					var notPermLabel = gen.DefineLabel();
					// Harmony replaces all short jumps with long jumps, never use short jumps
					// If you try brfalse.s (byte)1 it will crash because the branch gets made into a long jump
					// but your literal operand doesn't get moved.  Using labels when possible helps.
					codes.Insert(idx++, new CodeInstruction(OpCodes.Brfalse, notPermLabel));

					// load gravity transform
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_3));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldfld, Transform));

					// New position
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_S, 9));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldfld, X));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_S, 9));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldfld, Y));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_S, 4));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldfld, Z));

					codes.Insert(idx++, new CodeInstruction(OpCodes.Newobj, Vec3));

					// transform.SetPosition
					codes.Insert(idx++, new CodeInstruction(OpCodes.Call, SetTransform));
					// And ignore return value
					codes.Insert(idx++, new CodeInstruction(OpCodes.Pop));

					// this.data
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldarg_0));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldfld, Data));
					// [index] = gravityComponent
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_2));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldloc_3));
					codes.Insert(idx++, new CodeInstruction(OpCodes.Callvirt, ListSetIndex));

					codes.Insert(idx++, new CodeInstruction(OpCodes.Ret));
					codes[idx].labels.Add(notPermLabel);
					break;
				}
			}

			foreach(var codeInstruction in codes)
			{
				Debug.Log(codeInstruction);
			}

			return codes;
		}

		private static bool IsCellPermeable(GravityComponent comp, Vector2 pos2)
		{
			var go = Grid.Objects[Grid.PosToCell(pos2), (int)ObjectLayer.Building];
			var top = pos2;
			top.y += 2 * comp.radius;
			var go2 = Grid.Objects[Grid.PosToCell(top), (int)ObjectLayer.Building];
			return (go != null && go.GetComponent<ItemPermeableTile>() != null) ||
			       (go2 != null && go2.GetComponent<ItemPermeableTile>() != null);
		}
	}
}
