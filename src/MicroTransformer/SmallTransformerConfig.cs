using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace MicroTransformer;

public class SmallTransformerConfig : IBuildingConfig
{
	public const string Id = "asquared31415.MicroTransformer";

	public override BuildingDef CreateBuildingDef()
	{
		var buildingDef = BuildingTemplates.CreateBuildingDef(
			Id,
			2,
			1,
			"micro_transformer_kanim",
			BUILDINGS.HITPOINTS.TIER2,
			BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER0,
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
			MATERIALS.ALL_METALS,
			BUILDINGS.MELTING_POINT_KELVIN.TIER1,
			BuildLocationRule.Anywhere,
			BUILDINGS.DECOR.PENALTY.TIER1,
			NOISE_POLLUTION.NOISY.TIER0
		);

		buildingDef.RequiresPowerInput = true;
		buildingDef.RequiresPowerOutput = true;
		buildingDef.UseWhitePowerOutputConnectorColour = true;
		buildingDef.PowerInputOffset = new CellOffset(0, 0);
		buildingDef.PowerOutputOffset = new CellOffset(1, 0);
		buildingDef.ElectricalArrowOffset = new CellOffset(1, 0);
		buildingDef.ExhaustKilowattsWhenActive = 0.0f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.75f;
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.Entombable = true;
		buildingDef.GeneratorWattageRating = 750f;
		buildingDef.GeneratorBaseCapacity = 750f;
		buildingDef.PermittedRotations = PermittedRotations.R360;

		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefabTag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		var def = go.GetComponent<Building>().Def;
		var battery = go.AddOrGet<Battery>();
		battery.powerSortOrder = 1000;
		battery.capacity = def.GeneratorWattageRating;
		battery.chargeWattage = def.GeneratorWattageRating;
		go.AddComponent<PowerTransformer>().powerDistributionOrder = 9;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Object.DestroyImmediate(go.GetComponent<EnergyConsumer>());
		go.AddOrGetDef<PoweredActiveController.Def>();
	}
}