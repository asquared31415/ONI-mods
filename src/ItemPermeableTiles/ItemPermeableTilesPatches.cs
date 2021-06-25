using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CaiLib.Utils;
using Harmony;

namespace ItemPermeableTiles
{
	public class ItemPermeableTilesInfo
	{
		public static void OnLoad()
		{
			BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, ItemPermeableTileConfig.ID);

			LocString.CreateLocStringKeys(typeof(STRINGS), null);
		}
	}

	[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
	public static class TechAdd
	{
		public static void Postfix()
		{
			BuildingUtils.AddBuildingToTechnology(
				GameStrings.Technology.Liquids.Sanitation,
				ItemPermeableTileConfig.ID
			);
		}
	}

	[HarmonyPatch]
	public static class GravityPatches
	{
		private static readonly MethodInfo IsSolidHack = AccessTools.Method(typeof(GravityPatches), nameof(IsSolid));

		private static readonly MethodInfo SolidIndexer = AccessTools.Method(
			typeof(Grid.BuildFlagsSolidIndexer),
			"get_Item"
		);

		public static IEnumerable<MethodInfo> TargetMethods()
		{
			yield return AccessTools.Method(typeof(GravityComponents), "FixedUpdate");
			yield return AccessTools.Method(typeof(FallerComponents), "OnSolidChanged");
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
		{
			var codes = orig.ToList();
			for (var i = 0; i < codes.Count; ++i)
			{
				if (codes[i].operand is MethodInfo m && m == SolidIndexer)
				{
					codes[i] = new CodeInstruction(OpCodes.Call, IsSolidHack);
					codes.Insert(i - 1, new CodeInstruction(OpCodes.Pop));
					++i;
				}
			}

			return codes;
		}

		public static bool IsSolid(int cell)
		{
			// If solid and is not permeable, true, if permeable, false
			if (Grid.Solid[cell])
			{
				var go = Grid.Objects[cell, (int) ObjectLayer.Building];
				return go == null || go.GetComponent<ItemPermeableTile>() == null;
			}

			return false;
		}
	}
}
