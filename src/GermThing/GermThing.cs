using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using KMod;
using UnityEngine;

namespace GermThing;

[UsedImplicitly]
public class GermThing : UserMod2
{
}

public class GermOverlayMode : OverlayModes.Mode
{
	public static HashedString Id = "asquared31415_" + nameof(GermOverlayMode);

	public override HashedString ViewMode()
	{
		return Id;
	}

	public override string GetSoundName()
	{
		return "Decor";
	}
}

// The overlay code
[HarmonyPatch(typeof(SimDebugView), "OnPrefabInit")]
public static class SimDebugView_OnPrefabInit_Patch
{
	[UsedImplicitly]
	public static void Postfix(Dictionary<HashedString, Func<SimDebugView, int, Color>> ___getColourFuncs)
	{
		___getColourFuncs.Add(
			GermOverlayMode.Id,
			(view, cell) =>
			{
				var color = (Color) AccessTools.Method(typeof(SimDebugView), "GetDiseaseColour")
					.Invoke(null, new object[] { view, cell });
				color.a = (Grid.DiseaseCount[cell] <= 0) || (Grid.DiseaseIdx[cell] == byte.MaxValue) ? 0 : 1;
				return color;
			}
		);
	}
}

// Register in the overlay
[HarmonyPatch(typeof(OverlayMenu), "InitializeToggles")]
public static class OverlayMenu_InitializeToggles_Patch
{
	[UsedImplicitly]
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
				typeof(string),
			}
		);
		var _ = Action.NumActions.ToString();

		var obj = constructor.Invoke(
			new object[]
			{
				"Germ Overlay",
				"overlay_disease",
				GermOverlayMode.Id,
				"",
				Action.NumActions,
				"Displays germs betterer",
				"Germ Overlay",
			}
		);

		___overlayToggleInfos.Add((KIconToggleMenu.ToggleInfo) obj);
	}
}

[HarmonyPatch(typeof(OverlayScreen), "RegisterModes")]
public static class OverlayScreen_RegisterModes_Patch
{
	[UsedImplicitly]
	public static void Postfix()
	{
		var overlayScreen = Traverse.Create(OverlayScreen.Instance);
		overlayScreen.Method("RegisterMode", new GermOverlayMode()).GetValue();
	}
}

[HarmonyPatch(typeof(StatusItem), "GetStatusItemOverlayBySimViewMode")]
public static class StatusItem_GetStatusItemOverlayBySimViewMode_Patch
{
	[UsedImplicitly]
	public static void Prefix(Dictionary<HashedString, StatusItem.StatusItemOverlays> ___overlayBitfieldMap)
	{
		if (!___overlayBitfieldMap.ContainsKey(GermOverlayMode.Id))
		{
			___overlayBitfieldMap.Add(GermOverlayMode.Id, StatusItem.StatusItemOverlays.None);
		}
	}
}
