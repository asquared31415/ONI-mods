using System.IO;
using SquareLib;
using TUNING;
using UnityEngine;

namespace ItemPermeableTiles
{
	public class ItemPermeableTileConfig : IBuildingConfig
	{
		public const string ID = "asquared31415_ItemPermeableTileConfig";
		private const string ANIM = "floor_permeable_kanim";

		private static readonly int BlockTileConnectorId = Hash.SDBMLower("tiles_mesh_tops");

		public override BuildingDef CreateBuildingDef()
		{
			var buildingDef = BuildingTemplates.CreateBuildingDef(
				ID,
				1,
				1,
				ANIM,
				100,
				3f,
				BUILDINGS.CONSTRUCTION_MASS_KG.TIER3,
				MATERIALS.REFINED_METALS,
				1600f,
				BuildLocationRule.Tile,
				BUILDINGS.DECOR.NONE,
				NOISE_POLLUTION.NONE
			);

			BuildingTemplates.CreateFoundationTileDef(buildingDef);
			buildingDef.Floodable = false;
			buildingDef.Overheatable = false;
			buildingDef.Entombable = false;
			buildingDef.UseStructureTemperature = false;
			buildingDef.AudioCategory = "Metal";
			buildingDef.AudioSize = "small";
			buildingDef.BaseTimeUntilRepair = -1f;
			buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
			buildingDef.isKAnimTile = true;
			buildingDef.ConstructionOffsetFilter = BuildingDef.ConstructionOffsetFilter_OneDown;
			buildingDef.BlockTileIsTransparent = true;

			buildingDef.BlockTileMaterial = Assets.GetMaterial("tiles_solid");

			buildingDef.BlockTileAtlas
				= ModAssets.GetCustomTileAtlas(Path.Combine("anim", "assets", "tiles_permeable"));

			buildingDef.BlockTilePlaceAtlas
				= ModAssets.GetCustomTileAtlas(Path.Combine("anim", "assets", "tiles_permeable_place"));

			buildingDef.DecorBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_mesh_tops_decor_info");
			buildingDef.DecorPlaceBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_mesh_tops_decor_place_info");
			buildingDef.DragBuild = true;

			return buildingDef;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefabTag)
		{
			GeneratedBuildings.MakeBuildingAlwaysOperational(go);
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefabTag);
			var sco = go.AddOrGet<SimCellOccupier>();
			sco.doReplaceElement = false;
			sco.setTransparent = true;
			go.AddOrGet<TileTemperature>();
			go.AddOrGet<KAnimGridTileVisualizer>().blockTileConnectorID = BlockTileConnectorId;
			go.AddOrGet<BuildingHP>().destroyOnDamaged = true;
		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			GeneratedBuildings.RemoveLoopingSounds(go);
			go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles);
			go.AddComponent<SimTemperatureTransfer>();
			go.AddComponent<ZoneTile>();
			go.AddOrGet<ItemPermeableTile>();
		}

		public override void DoPostConfigureUnderConstruction(GameObject go)
		{
			base.DoPostConfigureUnderConstruction(go);
			go.AddOrGet<KAnimGridTileVisualizer>();
		}
	}
}
