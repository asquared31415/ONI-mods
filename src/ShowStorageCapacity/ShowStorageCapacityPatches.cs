using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using KMod;
using STRINGS;
using UnityEngine;

namespace ShowStorageCapacity;

[UsedImplicitly]
public class StorageCapacityInfo : UserMod2;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(SimpleInfoScreen), "RefreshStoragePanel")]
public class SimpleInfoScreen_RefreshStoragePanel_Patches
{
	[UsedImplicitly]
	public static void Postfix(CollapsibleDetailContentPanel targetPanel, [CanBeNull] GameObject targetEntity)
	{
		if (targetEntity != null)
		{
			// TODO: At some point adjust for dupe carrying capacity
			var storages = targetEntity.GetComponentsInChildren<IStorage>();
			var remainingCapacity = storages.Sum(static storage => storage.RemainingCapacity());
			var totalCapacity = storages.Sum(static storage => storage.Capacity());
			targetPanel.HeaderLabel.text +=
				": " + GameUtil.GetFormattedMass(totalCapacity - remainingCapacity) + " / " +
				GameUtil.GetFormattedMass(totalCapacity);
		}
	}
}
// ReSharper restore InconsistentNaming
