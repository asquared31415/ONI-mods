using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace ClothingLocker
{
    public class ClothingLockerConfig : IBuildingConfig
    {
        public const string Id = "asquared31415_" + nameof(ClothingLockerConfig);
        public const string Anim = "setpiece_locker_kanim";
        public const string Name = "Clothing Locker";
        public const string Effect = "Stores the clothing of your choosing.";

        public const string Desc = "Duplicants decided that putting clothes in with their debris was a bad idea.  " +
                                   "So they invented a storage bin specifically for storing clothing!";

        public override BuildingDef CreateBuildingDef()
        {
            var buildingDef = BuildingTemplates.CreateBuildingDef(
                Id,
                1,
                2,
                Anim,
                50,
                BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER2,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
                MATERIALS.RAW_METALS,
                BUILDINGS.MELTING_POINT_KELVIN.TIER1,
                BuildLocationRule.OnFloor,
                DECOR.PENALTY.TIER1,
                NOISE_POLLUTION.NONE
            );

            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefabTag)
        {
            SoundEventVolumeCache.instance.AddVolume(
                "storagelocker_kanim",
                "StorageLocker_Hit_metallic_low",
                NOISE_POLLUTION.NOISY.TIER1
            );

            Prioritizable.AddRef(go);
            var storage = go.AddOrGet<Storage>();
            storage.showInUI = true;
            storage.allowItemRemoval = true;
            storage.showDescriptor = true;
            storage.storageFilters = new List<Tag> {GameTags.Clothes};
            storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = Id;
            go.AddOrGet<StorageLocker>();
            go.AddOrGet<UserNameable>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }
}
