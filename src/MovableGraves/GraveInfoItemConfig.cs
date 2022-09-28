using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace MovableGraves;

[UsedImplicitly]
public class GraveInfoItemConfig : IEntityConfig
{
	public const string ID = "MovableGraves.GraveInfoItemConfig";
	public static readonly Tag Tag = TagManager.Create(ID);
	public const string Anim = "movablegraves_paper_kanim";

	public GameObject CreatePrefab()
	{
		var go = EntityTemplates.CreateLooseEntity(
			ID,
			"Headstone Record",
			"A record of what was once on a Duplicant's headstone. Dupes will automatically attempt to restore this information to another empty headstone.",
			1f,
			true,
			Assets.GetAnim(Anim),
			"object",
			Grid.SceneLayer.Front,
			EntityTemplates.CollisionShape.RECTANGLE,
			0.5f,
			0.5f,
			true,
			element: SimHashes.Granite,
			additionalTags: new List<Tag> { GameTags.Corpse, GameTags.MedicalSupplies }
		);
		go.AddOrGet<GraveInfoItem>();
		return go;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}

	public string[] GetDlcIds()
	{
		return DlcManager.AVAILABLE_ALL_VERSIONS;
	}
}
