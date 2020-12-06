using System.Linq;
using Harmony;
using UnityEngine;

namespace ShowStorageCapacity
{
    [HarmonyPatch(typeof(SimpleInfoScreen), "RefreshStorage")]
    public class SimpleInfoScreen_RefreshStorage_Patches
    {
        public static void Postfix(GameObject ___storagePanel, GameObject ___selectedTarget)
        {
            if(___selectedTarget != null)
            {
                var storages = ___selectedTarget.GetComponentsInChildren<Storage>();
                var panel = ___storagePanel.GetComponent<CollapsibleDetailContentPanel>();
                if(panel != null && storages.Length > 0)
                {
                    var storedMass = storages.Sum(storage => storage.MassStored());
                    var totalMass = storages.Sum(storage => storage.Capacity());
                    panel.HeaderLabel.text +=
                        ": " +
                        string.Format(STRINGS.UI.STARMAP.STORAGESTATS.STORAGECAPACITY, storedMass, totalMass) +
                        STRINGS.UI.UNITSUFFIXES.MASS.KILOGRAM;
                }
            }
        }
    }
}
