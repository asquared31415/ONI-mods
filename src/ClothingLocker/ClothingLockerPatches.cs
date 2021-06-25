using CaiLib.Utils;
using TUNING;

namespace ClothingLocker
{
	public class ClothingLockerInfo
	{
		public static void OnLoad()
		{
			ModUtil.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Furniture, ClothingLockerConfig.Id);

			StringUtils.AddBuildingStrings(
				ClothingLockerConfig.Id,
				ClothingLockerConfig.Name,
				ClothingLockerConfig.Desc,
				ClothingLockerConfig.Effect
			);

			STORAGEFILTERS.NOT_EDIBLE_SOLIDS.Remove(GameTags.Clothes);
		}
	}
}
