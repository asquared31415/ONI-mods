using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CaiLib.Utils;
using Harmony;

namespace InfiniteStorage
{
    public class InfiniteStoragePatches
    {
        public static void OnLoad()
        {
            CaiLib.Logger.Logger.LogInit();

            BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, DeepItemStorage.Id);
            BuildingUtils.AddBuildingToTechnology("SolidManagement", DeepItemStorage.Id);

            BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, DeepLiquidStorage.Id);
            BuildingUtils.AddBuildingToTechnology(GameStrings.Technology.Liquids.LiquidTuning, DeepLiquidStorage.Id);

            BuildingUtils.AddBuildingToPlanScreen(GameStrings.PlanMenuCategory.Base, DeepGasStorage.Id);
            BuildingUtils.AddBuildingToTechnology(GameStrings.Technology.Gases.HVAC, DeepGasStorage.Id);

            LocString.CreateLocStringKeys(typeof(STRINGS), null);
        }

        // If there is a button to specifically show/hide the contents of the container, enable the UI to show the
        // side screen, then reset it to the state of the button.
        // This prevents the resource screen from showing (and lagging), but keeps the filter screen visible.
        [HarmonyPatch(typeof(TreeFilterableSideScreen), nameof(TreeFilterableSideScreen.SetTarget))]
        public class TreeFilterableSideScreen_SetTarget_Patches
        {
            public static void Prefix(Storage ___storage)
            {
                if(___storage != null && ___storage.gameObject.GetComponent<ShowHideContentsButton>() != null)
                {
                    ___storage.showInUI = true;
                }
            }

            public static void Postfix(Storage ___storage)
            {
                if(___storage != null)
                {
                    var show = ___storage.gameObject.GetComponent<ShowHideContentsButton>();
                    if(show != null)
                    {
                        ___storage.showInUI = show.showContents;
                    }
                }
            }
        }

        /*
        [HarmonyPatch(typeof(TreeFilterableSideScreen), "CreateCategories")]
        public class TreeFilterableSideScreen_CreateCategories_Patches
        {
            // Insert an infinite storage check that sets a flag if true
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
            {
                List<CodeInstruction> codes = orig.ToList();

                var i = codes.FindIndex(ci => ci.opcode == OpCodes.Stloc_0);
                if(i != -1)
                {
                    // Load this.target
                    var target = AccessTools.Field(typeof(TreeFilterableSideScreen), "target");
                    codes.Insert(i++, new CodeInstruction(OpCodes.Ldarg_0));
                    codes.Insert(i++, new CodeInstruction(OpCodes.Ldfld, target));

                    // this.target.GetComponent<InfiniteStorage>();
                    var getInfStorage = AccessTools.Method(typeof(GameObject), "GetComponent");
                    getInfStorage = getInfStorage.MakeGenericMethod(typeof(InfiniteStorage));
                    codes.Insert(i++, new CodeInstruction(OpCodes.Callvirt, getInfStorage));

                    // != null
                    codes.Insert(i++, new CodeInstruction(OpCodes.Ldnull));
                    var goInequality = AccessTools.Method(
                        typeof(Object),
                        "op_Inequality",
                        new[] { typeof(Object), typeof(Object) }
                    );

                    codes.Insert(i++, new CodeInstruction(OpCodes.Call, goInequality));
                    codes.Insert(i++, new CodeInstruction(OpCodes.Or));
                    return codes;
                }

                Debug.LogWarning("[InfiniteStorage] Unable to patch TreeFilterableSideScreen.CreateCategories");
                return codes;
            }
        }

        [HarmonyPatch(typeof(TreeFilterableSideScreen), "AddRow")]
        public class TreeFilterableSideScreen_AddRow_Patches
        {
            // Add every prefab for the filters
            // There's some duplication that is very D:
            public static void Postfix(
                TreeFilterableSideScreen __instance,
                TreeFilterableSideScreenRow __result,
                UIPool<TreeFilterableSideScreenRow> ___rowPool,
                Tag rowTag
            )
            {
                var instance = Traverse.Create(__instance);
                var target = (GameObject) instance.Field("target").GetValue();
                if(target == null || target.GetComponent<InfiniteStorage>() == null)
                {
                    return;
                }

                var targetFilterable = (TreeFilterable) instance.Field("targetFilterable").GetValue();
                var map = new Dictionary<Tag, bool>();
                foreach(var go in Assets.GetPrefabsWithTag(rowTag))
                {
                    var prefab = go.GetComponent<KPrefabID>();
                    if(prefab.GetComponent<Pickupable>() != null)
                    {
                        var element = ElementLoader.GetElement(prefab.PrefabTag);
                        if(element != null)
                        {
                            if(!element.disabled && element.materialCategory == rowTag)
                            {
                                map.Add(
                                    element.tag,
                                    targetFilterable.ContainsTag(element.tag) || targetFilterable.ContainsTag(rowTag)
                                );
                            }
                        }
                        else
                        {
                            map.Add(
                                prefab.PrefabTag,
                                targetFilterable.ContainsTag(prefab.PrefabTag) || targetFilterable.ContainsTag(rowTag)
                            );
                        }
                    }
                }

                if(map.Count > 0)
                {
                    __result.SetElement(rowTag, targetFilterable.ContainsTag(rowTag), map);
                }
                else
                {
                    ___rowPool.ClearElement(__result);
                }
            }
        }*/

        [HarmonyPatch(typeof(ConduitConsumer), "Consume")]
        public class ConduitConsumer_Consume_Patches
        {
            public static bool Prefix(ConduitConsumer __instance, ConduitFlow conduit_mgr, int ___utilityCell)
            {
                // if we don't have the component, do nothing special
                if(__instance.gameObject.GetComponent<InfiniteStorage>() == null)
                {
                    return true;
                }

                var contents = conduit_mgr.GetContents(___utilityCell);

                var storage = __instance.gameObject.GetComponent<Storage>();
                if(storage == null)
                {
                    return true;
                }

                var filterable = __instance.gameObject.GetComponent<TreeFilterable>();
                if(filterable == null)
                {
                    return true;
                }

                // If it doesn't contain the tag, return false, don't consume
                var tag = ElementLoader.FindElementByHash(contents.element).tag;
                var ret = filterable.AcceptedTags.Contains(tag);
                return ret;
            }
        }

        [HarmonyPatch(typeof(Storage), "AddLiquid")]
        public class Storage_AddLiquid_Patches
        {
            public static void Prefix(
                SimHashes element,
                ref float mass,
                float temperature,
                byte disease_idx,
                int disease_count,
                bool keep_zero_mass,
                bool do_disease_transfer,
                Storage __instance
            )
            {
                // We don't have any more space for that much mass
                var primaryStored = __instance.FindPrimaryElement(element);
                // Let default behavior if not exists
                if(primaryStored == null)
                {
                    return;
                }

                var massAvailable = PrimaryElement.MAX_MASS - primaryStored.Mass;
                if(massAvailable <= mass)
                {
                    var overflowMass = mass - massAvailable;
                    mass = massAvailable;

                    var chunk = LiquidSourceManager.Instance.CreateChunk(
                        element,
                        overflowMass,
                        temperature,
                        disease_idx,
                        disease_count,
                        __instance.transform.GetPosition()
                    );

                    chunk.GetComponent<PrimaryElement>().KeepZeroMassObject = keep_zero_mass;
                    __instance.Store(chunk.gameObject, true, false, do_disease_transfer);
                }
            }
        }

        [HarmonyPatch(typeof(Storage), "AddGasChunk")]
        public class Storage_AddGasChunk_Patches
        {
            public static void Prefix(
                SimHashes element,
                ref float mass,
                float temperature,
                byte disease_idx,
                int disease_count,
                bool keep_zero_mass,
                bool do_disease_transfer,
                Storage __instance
            )
            {
                // We don't have any more space for that much mass
                var primaryStored = __instance.FindPrimaryElement(element);
                // Let default behavior if not exists
                if(primaryStored == null)
                {
                    return;
                }

                var massAvailable = PrimaryElement.MAX_MASS - primaryStored.Mass;
                if(massAvailable <= mass)
                {
                    var overflowMass = mass - massAvailable;
                    mass = massAvailable;

                    var chunk = GasSourceManager.Instance.CreateChunk(
                        element,
                        overflowMass,
                        temperature,
                        disease_idx,
                        disease_count,
                        __instance.transform.GetPosition()
                    );

                    chunk.GetComponent<PrimaryElement>().KeepZeroMassObject = keep_zero_mass;
                    __instance.Store(chunk.gameObject, true, false, do_disease_transfer);
                }
            }
        }

        [HarmonyPatch(typeof(Storage), nameof(Storage.MassStored))]
        public class Storage_MassStored_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
            {
                List<CodeInstruction> codes = codeInstructions.ToList();

                for(var i = 0; i < codes.Count; ++i)
                {
                    var ci = codes[i];
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if(ci.opcode == OpCodes.Ldc_R4 && (float) ci.operand == 1_000f)
                    {
                        // This replaces the code that multiplies and divides by 1000
                        // with code that just uses Math.Round(num, 3)
                        // why didn't you do this Klei?
                        codes[i] = new CodeInstruction(OpCodes.Ldc_I4_3);
                        codes[i + 1] = new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.Method(typeof(Math), "Round", new[] { typeof(double), typeof(int) })
                        );

                        codes[i + 2] = new CodeInstruction(OpCodes.Conv_R4);
                        codes[i + 3] = new CodeInstruction(OpCodes.Ret);

                        codes.RemoveRange(i + 4, codes.Count - (i + 4));
                        return codes;
                    }
                }

                Debug.LogWarning("[InfiniteStorage] Unable to patch storage display mass");
                return codes;
            }
        }
    }
}
