using HarmonyLib;
using JetBrains.Annotations;
using KMod;
using UnityEngine;

namespace SpicedFoodFilter;

[UsedImplicitly]
public class SpicedFoodFilter : UserMod2
{
}

[HarmonyPatch(
	typeof(FilteredStorage),
	MethodType.Constructor,
	typeof(KMonoBehaviour),
	typeof(Tag[]),
	typeof(IUserControlledCapacity),
	typeof(bool),
	typeof(ChoreType)
)]
public static class FilteredStorage_Ctor_Patch
{
	[UsedImplicitly]
	public static void Postfix(FilteredStorage __instance, KMonoBehaviour root)
	{
		// only add to things that aren't a FoodStorage or spice grinder
		if ((root.gameObject.GetComponent<FoodStorage>() == null) &&
			(root.gameObject.GetComponent<SpiceGrinderWorkable>() == null))
		{
			var spiceFilter = root.gameObject.AddOrGet<SpiceFilter>();
			spiceFilter.FilteredStorage = __instance;
		}
	}
}

[HarmonyPatch(typeof(TreeFilterableSideScreen), "OnOnlySpicedItemsSettingChanged")]
public static class TreeFilterableSideScreen_OnOnlySpicedItemsSettingChanged_Patch
{
	[UsedImplicitly]
	public static void Postfix(
		Storage ___storage,
		GameObject ___onlyallowSpicedItemsRow,
		MultiToggle ___onlyAllowSpicedItemsCheckBox
	)
	{
		// SpiceFilter is only ever added to things that don't already have a FoodStorage, so it can never conflict
		if (___storage.TryGetComponent<SpiceFilter>(out var spiceFilter))
		{
			___onlyallowSpicedItemsRow.SetActive(true);
			___onlyAllowSpicedItemsCheckBox.ChangeState(spiceFilter.OnlyStoreSpicedFood ? 1 : 0);
		}
	}
}

[HarmonyPatch(typeof(TreeFilterableSideScreen), "OnlyAllowSpicedItemsClicked")]
public static class TreeFilterableSideScreen_OnlyAllowSpicedItemsClicked_Patch
{
	[UsedImplicitly]
	public static bool Prefix(Storage ___storage)
	{
		if (___storage.TryGetComponent<SpiceFilter>(out var spiceFilter))
		{
			spiceFilter.SetStoreSpiced(!spiceFilter.OnlyStoreSpicedFood);
			// skip original code that assumes that it has a FoodStorage
			return false;
		}

		return true;
	}
}
