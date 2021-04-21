using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using ProcGenGame;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RenameAsteroids
{
    [HarmonyPatch(typeof(WorldSelector), "AddWorld")]
    public static class WorldSelector_AddWorld_Patch
    {
        public static void Postfix(WorldSelector __instance, object data)
        {
            var key = (int) data;
            var addedWorld = __instance.worldRows.FirstOrDefault(e => e.Key == key);
            addedWorld.Value.onDoubleClick = () => RenameAsteroids.OnDoubleClick(addedWorld);
        }
    }

    [HarmonyPatch(typeof(WorldSelector), "SpawnToggles")]
    public static class WorldSelector_SpawnToggles_Patch
    {
        public static void Postfix(WorldSelector __instance)
        {
            foreach (var worldRow in __instance.worldRows)
            {
                worldRow.Value.onDoubleClick = () => RenameAsteroids.OnDoubleClick(worldRow);
            }
        }
    }

    [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        public static void Postfix(List<DetailsScreen.SideScreenRef> ___sideScreens)
        {
            var alarmSideScreen = ___sideScreens.FirstOrDefault(s => s.name == "Alarm SideScreen");
            if (alarmSideScreen != null)
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

        public static bool OnDoubleClick(KeyValuePair<int, MultiToggle> row)
        {
            var world = ClusterManager.Instance.GetWorld(row.Key);
            var gridEntity = world.GetComponent<ClusterGridEntity>();
            if (gridEntity == null)
            {
                return false;
            }

            switch (gridEntity)
            {
                case AsteroidGridEntity _:
                {
                    var hierarchy = row.Value.GetComponent<HierarchyReferences>();
                    var label = hierarchy.GetReference<LocText>("Label");
                    var input = Object.Instantiate(InputPrefab, label.transform);
                    input.field.text = gridEntity.Name;
                    input.field.fontAsset = label.font;
                    input.field.ActivateInputField();
                    input.onEndEdit += () =>
                                       {
                                           var trav = Traverse.Create(gridEntity);
                                           trav.Field("m_name").SetValue(input.field.text);
                                           trav.Field<KSelectable>("m_selectable").Value.SetName(input.field.text);
                                           label.enabled = true;

                                           Game.Instance.Trigger((int) GameHashes.DiscoveredWorldsChanged);
                                           Object.Destroy(input);
                                       };

                    label.enabled = false;
                    return true;
                }
                case Clustercraft c:
                {
                    var hierarchy = row.Value.GetComponent<HierarchyReferences>();
                    var label = hierarchy.GetReference<LocText>("Label");
                    var input = Object.Instantiate(InputPrefab, label.transform);
                    input.field.text = gridEntity.Name;
                    input.field.fontAsset = label.font;
                    input.field.ActivateInputField();
                    input.onEndEdit += () =>
                                       {
                                           c.SetRocketName(input.field.text);

                                           var trav = Traverse.Create(gridEntity);
                                           trav.Field<KSelectable>("m_selectable").Value.SetName(input.field.text);
                                           label.enabled = true;

                                           Game.Instance.Trigger((int) GameHashes.DiscoveredWorldsChanged);
                                           Object.Destroy(input);
                                       };

                    label.enabled = false;
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}
