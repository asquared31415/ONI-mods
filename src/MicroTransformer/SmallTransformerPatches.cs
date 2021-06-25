using CaiLib.Utils;
using HarmonyLib;
using KMod;

namespace MicroTransformer
{
	public class SmallTransformerInfo : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Power, SmallTransformerConfig.Id);

			StringUtils.AddBuildingStrings(
				SmallTransformerConfig.Id,
				SmallTransformerConfig.DisplayName,
				SmallTransformerConfig.Description,
				SmallTransformerConfig.Effect
			);
			base.OnLoad(harmony);
		}
	}

	[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
	public static class SmallTransformerTech
	{
		public static void Postfix()
		{
			BuildingUtils.AddBuildingToTechnology(
				GameStrings.Technology.Power.AdvancedPowerRegulation,
				SmallTransformerConfig.Id
			);
		}
	}
}
