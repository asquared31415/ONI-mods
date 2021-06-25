using System.Linq;
using HarmonyLib;
using KMod;
using STRINGS;
using UnityEngine;

namespace ShowStorageCapacity
{
	public class StorageCapacityInfo : UserMod2
	{
	}

	[HarmonyPatch(typeof(SimpleInfoScreen), "RefreshStorage")]
	public class SimpleInfoScreen_RefreshStorage_Patches
	{
		public static void Postfix(SimpleInfoScreen __instance, GameObject ___selectedTarget)
		{
			if (___selectedTarget != null)
			{
				// TODO: At some point adjust for dupe carrying capacity
				var storages = ___selectedTarget.GetComponentsInChildren<Storage>();
				var panel = __instance.StoragePanel.GetComponent<CollapsibleDetailContentPanel>();
				if (panel != null && storages.Length > 0)
				{
					var storedMass = storages.Sum(storage => storage.MassStored());
					var totalMass = storages.Sum(storage => storage.Capacity());
					panel.HeaderLabel.text
						+= ": " + string.Format(UI.STARMAP.STORAGESTATS.STORAGECAPACITY, storedMass, totalMass) +
						   UI.UNITSUFFIXES.MASS.KILOGRAM;
				}
			}
		}
	}
}
