using HarmonyLib;
using KMod;

namespace MicroTransformer
{
	public class SmallTransformerInfo : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			ModUtil.AddBuildingToPlanScreen("Power", SmallTransformerConfig.Id);
			LocString.CreateLocStringKeys(typeof(MicroTransformerStrings.STRINGS), null);
			base.OnLoad(harmony);
		}
	}

	[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
	public static class SmallTransformerTech
	{
		public static void Postfix()
		{
			Db.Get().Techs.Get("AdvancedPowerRegulation").unlockedItemIDs.Add(SmallTransformerConfig.Id);
		}
	}
}
