using HarmonyLib;
using JetBrains.Annotations;
using Klei.CustomSettings;
using KMod;

namespace StartWithResearch;

public class StartWithResearch : UserMod2
{
}

[HarmonyPatch(typeof(CustomGameSettings), "OnPrefabInit")]
public static class CustomGameSettings_OnPrefabInit_Patch
{
	public static readonly SettingConfig StartResearchSetting = new ToggleSettingConfig(
		"asquared31415.StartWithResearch",
		"Start With All Research",
		"Start the game with all researches completed.",
		new SettingLevel(
			"Disabled",
			"Disabled",
			"Unchecked: Research will need to be completed using research stations (Default)"
		),
		new SettingLevel("Enabled", "Enabled", "Checked: All research will be unlocked when starting the game"),
		"Disabled",
		"Disabled"
	);

	[UsedImplicitly]
	public static void Postfix(CustomGameSettings __instance)
	{
		__instance.AddQualitySettingConfig(StartResearchSetting);
	}
}

[HarmonyPatch(typeof(Game), "OnSpawn")]
public static class Game_OnSpawn_Patch
{
	[UsedImplicitly]
	public static void Postfix()
	{
		if (CustomGameSettings.Instance.GetCurrentQualitySetting(
					CustomGameSettings_OnPrefabInit_Patch.StartResearchSetting
				)
				.id == "Enabled")
		{
			foreach (var tech in Db.Get().Techs.resources)
			{
				Research.Instance.SetActiveResearch(tech);
				Research.Instance.CompleteQueue();
			}
		}
	}
}

// Disable the achievement
[HarmonyPatch(typeof(ColonyAchievementTracker), "OnSpawn")]
public static class ColonyAchievementTracker_OnSpawn_Patch
{
	[UsedImplicitly]
	public static void Postfix(ColonyAchievementTracker __instance)
	{
		if (CustomGameSettings.Instance.GetCurrentQualitySetting(
					CustomGameSettings_OnPrefabInit_Patch.StartResearchSetting
				)
				.id == "Enabled")
		{
			__instance.achievements[Db.Get().ColonyAchievements.CompleteResearchTree.Id].failed = true;
		}
	}
}
