using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace InfiniteStorage
{
	public class DeepItemStorage : IBuildingConfig
	{
		public const string Id = "InfStorage_InfiniteItemStorage";
		private const string Anim = "liquidreservoir_kanim";

		public override BuildingDef CreateBuildingDef()
		{
			var buildingDef = BuildingTemplates.CreateBuildingDef(
				Id,
				2,
				3,
				Anim,
				3,
				60f,
				BUILDINGS.CONSTRUCTION_MASS_KG.TIER5,
				MATERIALS.REFINED_METALS,
				1_600f,
				BuildLocationRule.OnFloor,
				BUILDINGS.DECOR.PENALTY.TIER1,
				NOISE_POLLUTION.NONE
			);

			buildingDef.Floodable = false;
			buildingDef.AudioCategory = "HollowMetal";
			buildingDef.Overheatable = false;
			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			Prioritizable.AddRef(go);
			go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
			var storage = go.AddOrGet<Storage>();
			storage.capacityKg = float.PositiveInfinity;
			storage.allowItemRemoval = true;
			storage.SetDefaultStoredItemModifiers(
				new List<Storage.StoredItemModifier>
				{
					Storage.StoredItemModifier.Insulate,
					Storage.StoredItemModifier.Hide,
					Storage.StoredItemModifier.Seal,
				}
			);

			storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS;
			storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
			storage.showInUI = false;
			go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.StorageLocker;
			go.AddOrGet<InfiniteStorage>();
			go.AddOrGet<ShowHideContentsButton>();
			go.AddOrGet<UserNameable>();
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGetDef<StorageController.Def>();
		}
	}
}
