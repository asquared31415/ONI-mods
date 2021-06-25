using CaiLib.Utils;
using Harmony;

namespace MicroTransformer
{
	public class SmallTransformerInfo
	{
		public static void OnLoad()
		{
			BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Power, SmallTransformerConfig.Id);

			StringUtils.AddBuildingStrings(
				SmallTransformerConfig.Id,
				SmallTransformerConfig.DisplayName,
				SmallTransformerConfig.Description,
				SmallTransformerConfig.Effect
			);
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
