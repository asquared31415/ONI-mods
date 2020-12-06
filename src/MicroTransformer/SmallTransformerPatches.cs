using CaiLib.Utils;

namespace MicroTransformer
{
    public class SmallTransformerPatches
    {
        public static void OnLoad()
        {
            CaiLib.Logger.Logger.LogInit();

            BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Power, SmallTransformerConfig.Id);
            BuildingUtils.AddBuildingToTechnology(
                GameStrings.Technology.Power.PowerRegulation,
                SmallTransformerConfig.Id
            );

            StringUtils.AddBuildingStrings(
                SmallTransformerConfig.Id,
                SmallTransformerConfig.DisplayName,
                SmallTransformerConfig.Description,
                SmallTransformerConfig.Effect
            );
        }
    }
}
