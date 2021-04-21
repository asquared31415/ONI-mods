using System;
using System.Collections.Generic;
using Harmony;
using SquareLib;
using UnityEngine;

namespace TrafficVisualizer
{
    public static class TrafficVisualizerPatches
    {
        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Postfix() { ModAssets.AddSpriteFromFile("traffic_overlay_icon"); }
        }

        [HarmonyPatch(typeof(TransitionDriver), "EndTransition")]
        public static class TransitionDriver_EndTransition_Patch
        {
            public static void Prefix(Navigator ___navigator, Vector3 ___targetPos)
            {
                if(___navigator != null && ___navigator.gameObject.HasTag(GameTags.Minion))
                {
                    NavigatorRecordManager.Add(Grid.PosToCell(___targetPos));
                }
            }
        }

        [HarmonyPatch(typeof(Game), "OnDestroy")]
        public static class Game_OnDestroy_Patch
        {
            public static void Prefix() { NavigatorRecordManager.Clear(); }
        }

        // The overlay code
        [HarmonyPatch(typeof(SimDebugView), "OnPrefabInit")]
        public static class SimDebugView_OnPrefabInit_Patch
        {
            public static void Postfix(Dictionary<HashedString, Func<SimDebugView, int, Color>> ___getColourFuncs)
            {
                ___getColourFuncs.Add(HotspotOverlayMode.Id, NavigatorColors.GetNavColor);
            }
        }

        // Register in the overlay
        [HarmonyPatch(typeof(OverlayMenu), "InitializeToggles")]
        public static class OverlayMenu_InitializeToggles_Patch
        {
            public static void Postfix(List<KIconToggleMenu.ToggleInfo> ___overlayToggleInfos)
            {
                var constructor = AccessTools.Constructor(
                    AccessTools.Inner(typeof(OverlayMenu), "OverlayToggleInfo"),
                    new[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(HashedString),
                        typeof(string),
                        typeof(Action),
                        typeof(string),
                        typeof(string)
                    }
                );

                var obj = constructor.Invoke(
                    new object[]
                    {
                        "Traffic Overlay",
                        "traffic_overlay_icon",
                        HotspotOverlayMode.Id,
                        "",
                        Action.Overlay15,
                        "Displays areas where duplicants travel frequently {Hotkey}",
                        "Traffic Overlay"
                    }
                );

                ___overlayToggleInfos.Add((KIconToggleMenu.ToggleInfo) obj);
            }
        }

        [HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
        public static class OverlayScreen_RegisterModes_Patch
        {
            public static void Postfix()
            {
                var overlayScreen = Traverse.Create(OverlayScreen.Instance);
                overlayScreen.Method("RegisterMode", new HotspotOverlayMode()).GetValue();
            }
        }

        [HarmonyPatch(typeof(StatusItem), "GetStatusItemOverlayBySimViewMode")]
        public static class StatusItem_GetStatusItemOverlayBySimViewMode_Patch
        {
            public static void Prefix(Dictionary<HashedString, StatusItem.StatusItemOverlays> ___overlayBitfieldMap)
            {
                if(!___overlayBitfieldMap.ContainsKey(HotspotOverlayMode.Id))
                {
                    ___overlayBitfieldMap.Add(HotspotOverlayMode.Id, StatusItem.StatusItemOverlays.None);
                }
            }
        }

        [HarmonyPatch(typeof(GameClock), "OnPrefabInit")]
        public static class GameClock_OnPrefabInit_Patch
        {
            public static void Postfix(GameClock __instance)
            {
                __instance.Subscribe((int) GameHashes.NewDay, NavigatorRecordManager.UpdateCounts);
            }
        }
    }
}
