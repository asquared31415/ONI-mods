using CaiLib.Utils;
using HarmonyLib;
using KMod;
using TUNING;

namespace ClothingLocker
{
	public class ClothingLockerInfo : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			ModUtil.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Furniture, ClothingLockerConfig.Id);

			StringUtils.AddBuildingStrings(
				ClothingLockerConfig.Id,
				ClothingLockerConfig.Name,
				ClothingLockerConfig.Desc,
				ClothingLockerConfig.Effect
			);

			base.OnLoad(harmony);
		}
	}
}
