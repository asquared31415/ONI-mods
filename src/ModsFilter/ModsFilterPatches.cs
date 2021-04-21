using System.Globalization;
using Harmony;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ModsFilter
{
    public static class ModsFilterPatches
    {
        private static FilterManager _filterManager;
        private static ModsScreen _modsScreen;
        private static GameObject _prefab;

        [HarmonyPatch(typeof(MainMenu), "OnPrefabInit")]
        public static class MainMenu_OnPrefabInit_Patch
        {
            public static void Postfix()
            {
                var prefabGo = ScreenPrefabs.Instance.RetiredColonyInfoScreen.gameObject;
                prefabGo.SetActive(false);
                var clone = Util.KInstantiateUI(prefabGo);
                if(clone != null)
                {
                    var go = clone.transform.Find("Content/ColonyData/Colonies and Achievements/Colonies/Search");
                    if(go != null)
                    {
                        _prefab = Util.KInstantiateUI(go.gameObject);
                        Object.Destroy(clone);
                        prefabGo.SetActive(true);

                        return;
                    }
                }

                // ERROR!
                Debug.Log("[ModFilter] Error creating search prefab!  The mod will not function!");
            }
        }

        [HarmonyPatch(typeof(ModsScreen), "OnActivate")]
        public static class ModsScreen_OnActivate_Patch
        {
            public static void Prefix(ModsScreen __instance)
            {
                if(_prefab == null)
                {
                    return;
                }

                _modsScreen = __instance;
                var local = Util.KInstantiateUI(_prefab);
                var panel = __instance.transform.Find("Panel");
                if(panel != null)
                {
                    var trans = local.transform;
                    trans.SetParent(panel, false);
                    trans.SetSiblingIndex(1);
                    local.SetActive(true);

                    _filterManager = new FilterManager(
                        trans.Find("LocTextInputField").GetComponent<TMP_InputField>(),
                        trans.Find("ClearButton").GetComponent<KButton>()
                    );

                    _filterManager.ConfigureButtons(_modsScreen);
                }
                else
                {
                    Debug.Log("[ModFilter] Error adding search bar to mods screen!");
                }
            }
        }

        [HarmonyPatch(typeof(ModsScreen), "OnDeactivate")]
        public static class ModsScreen_OnDeactivate_Patch
        {
            public static void Prefix()
            {
                _modsScreen = null;
                _filterManager = null;
            }
        }

        // NOTE: This gets called within OnActivate as well
        [HarmonyPatch(typeof(ModsScreen), "ShouldDisplayMod")]
        public static class ModsScreen_ShouldDisplayMod_Patch
        {
            public static void Postfix(KMod.Mod mod, ref bool __result)
            {
                if(__result && _filterManager != null)
                {
                    var text = _filterManager.Text;
                    if(!string.IsNullOrEmpty(text))
                    {
                        __result = CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                                       mod.label.title,
                                       text,
                                       CompareOptions.IgnoreCase
                                   ) >=
                                   0;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ModsScreen), "UpdateToggleAllButton")]
        public static class ModsScreen_UpdateToggleAllButton_Patch
        {
            // If we are not searching, let the button update
            public static bool Prefix() => _filterManager == null || string.IsNullOrEmpty(_filterManager.Text);
        }

        [HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
        public static class ModsScreen_BuildDisplay_Patch
        {
            // Update the toggle button's state every time we build the display
            public static void Prefix(KButton ___toggleAllButton)
            {
                var isEmpty = _filterManager == null || string.IsNullOrEmpty(_filterManager.Text);
                ___toggleAllButton.isInteractable = isEmpty;
                if(!isEmpty)
                {
                    ___toggleAllButton.gameObject.AddOrGet<ToolTip>().toolTip = "Disabled while filter is active";
                }
                else
                {
                    var tt = ___toggleAllButton.gameObject.GetComponent<ToolTip>();
                    if(tt != null)
                    {
                        Object.Destroy(tt);
                    }
                }
            }
        }
    }
}
