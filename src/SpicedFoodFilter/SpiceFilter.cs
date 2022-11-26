using System.Linq;
using KSerialization;
using TUNING;
using UnityEngine;

namespace SpicedFoodFilter;

[SerializationConfig(MemberSerialization.OptIn)]
public class SpiceFilter : KMonoBehaviour
{
	[Serialize] [SerializeField] private bool onlyStoreSpicedFood;

#pragma warning disable CS0649
	[MyCmpReq] private Storage storage;
#pragma warning restore CS0649

	public bool OnlyStoreSpicedFood => onlyStoreSpicedFood;

	public FilteredStorage FilteredStorage;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Subscribe((int) GameHashes.CopySettings, OnCopySettings);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		// Update value on creation
		SetStoreSpiced(onlyStoreSpicedFood);
	}

	private void OnCopySettings(object data)
	{
		if (data is GameObject go)
		{
			if (go.TryGetComponent<SpiceFilter>(out var spiceFilter))
			{
				SetStoreSpiced(spiceFilter.onlyStoreSpicedFood);
			}
		}
	}

	private static readonly Tag[] FoodArr = STORAGEFILTERS.FOOD.ToArray();

	public void SetStoreSpiced(bool storeSpiced)
	{
		onlyStoreSpicedFood = storeSpiced;
		Trigger((int) GameHashes.OnlyFetchSpicedItemsSettingChanged);
		if (onlyStoreSpicedFood)
		{
			FilteredStorage.AddForbiddenTag(GameTags.UnspicedFood);

			// Drop all items that are food, but are not spiced food
			// note: this must be eagerly collected to a List to prevent the iterator being invalidated
			var toDrop = storage.items.Where(item => item.HasAnyTags(FoodArr) && !item.HasTag(GameTags.SpicedFood))
				.ToList();
			foreach (var o in toDrop)
			{
				storage.Drop(o);
			}

			storage.DropUnlessHasTag(GameTags.SpicedFood);
		}
		else
		{
			FilteredStorage.RemoveForbiddenTag(GameTags.UnspicedFood);
		}
	}
}
