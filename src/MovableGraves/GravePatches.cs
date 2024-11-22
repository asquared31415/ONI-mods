using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace MovableGraves;

public static class GravePatches
{
	[HarmonyPatch(typeof(GraveConfig), nameof(GraveConfig.ConfigureBuildingTemplate))]
	public static class GraveConfig_ConfigureBuildingTemplate_Patch
	{
		[UsedImplicitly]
		public static void Postfix(GameObject go)
		{
			go.AddOrGet<GraveDeconstructDropper>();
		}
	}

	[HarmonyPatch(typeof(Grave), "OnStorageChanged")]
	public static class Grave_StorageChanged_Patch
	{
		[UsedImplicitly]
		public static void Prefix(object data, Grave __instance)
		{
			if (data is not GameObject go)
			{
				return;
			}

			// copy the data over in 1 tick if it's a special info item
			if (go.TryGetComponent<GraveInfoItem>(out var infoItem))
			{
				var graveName = infoItem.graveName;
				var epitaphIdx = infoItem.epitaphIdx;
				var burialTime = infoItem.burialTime;
				var graveAnim = infoItem.graveAnim;
				GameScheduler.Instance.ScheduleNextFrame(
					"MovableGraves.FixGraveInfo",
					_ =>
					{
						__instance.graveName = graveName;
						__instance.epitaphIdx = epitaphIdx;
						__instance.burialTime = burialTime;
						__instance.graveAnim = graveAnim;

						// play the new grave anim
						if (__instance.TryGetComponent<KAnimControllerBase>(out var kacb))
						{
							kacb.Play(__instance.graveAnim);
						}
					}
				);
			}
		}
	}
}
