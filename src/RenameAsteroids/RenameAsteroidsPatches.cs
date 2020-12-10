using System.Collections.Generic;
using System.Linq;
using Harmony;
using UnityEngine;

namespace RenameAsteroids
{
    [HarmonyPatch(typeof(WorldSelector), "AddWorld")]
    public static class WorldSelector_AddWorld_Patch
    {
        public static void Postfix(WorldSelector __instance, object data)
        {
            var key = (int) data;
            var addedWorld = __instance.worldRows.FirstOrDefault(e => e.Value == key);
            addedWorld.Key.onDoubleClick = () => RenameAsteroids.OnDoubleClick(addedWorld);
        }
    }

    [HarmonyPatch(typeof(WorldSelector), "SpawnToggles")]
    public static class WorldSelector_SpawnToggles_Patch
    {
        public static void Postfix(WorldSelector __instance)
        {
            foreach(var worldRow in __instance.worldRows)
            {
                worldRow.Key.onDoubleClick = () => RenameAsteroids.OnDoubleClick(worldRow);
            }
        }
    }

    [HarmonyPatch(typeof(DetailsScreen),"OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        public static void Postfix(List<DetailsScreen.SideScreenRef> ___sideScreens)
        {
            var alarmSideScreen = ___sideScreens.FirstOrDefault(s => s.name == "Alarm SideScreen");
            if(alarmSideScreen != null)
            {
                var trav = Traverse.Create((AlarmSideScreen) alarmSideScreen.screenPrefab);
                var prefab = trav.Field<KInputField>("nameInputField").Value;
                RenameAsteroids.InputPrefab = Object.Instantiate(prefab);
                var trans = RenameAsteroids.InputPrefab.GetComponent<RectTransform>();
                trans.anchorMax = Vector2.one;
            }
        }
    }

    public static class RenameAsteroids
    {
        public static KInputField InputPrefab;

        public static bool OnDoubleClick(KeyValuePair<MultiToggle, int> row)
        {
            var world = ClusterManager.Instance.GetWorld(row.Value);
            var asteroid = world.GetComponent<AsteroidGridEntity>();
            if(asteroid == null)
            {
                return false;
            }

            var hierarchy = row.Key.GetComponent<HierarchyReferences>();
            var label = hierarchy.GetReference<LocText>("Label");
            var input = Object.Instantiate(InputPrefab, label.transform);
            input.field.text = asteroid.Name;
            input.field.fontAsset = label.font;
            input.field.ActivateInputField();
            input.onEndEdit += () =>
            {
                var trav = Traverse.Create(asteroid);
                trav.Field("m_name").SetValue(input.field.text);
                trav.Field<KSelectable>("m_selectable").Value.SetName(input.field.text);
                label.enabled = true;
                Game.Instance.Trigger((int) GameHashes.DiscoveredWorldsChanged);
                Object.Destroy(input);
            };

            label.enabled = false;
            return true;
        }
    }
}
