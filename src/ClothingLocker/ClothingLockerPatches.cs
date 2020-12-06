using CaiLib.Utils;
using TUNING;

namespace ClothingLocker
{
    public class ClothingLockerPatches
    {
        public static void OnLoad()
        {
            CaiLib.Logger.Logger.LogInit();

            BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, ClothingLockerConfig.Id);

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
