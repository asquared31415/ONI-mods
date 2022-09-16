using HarmonyLib;
using JetBrains.Annotations;
using KMod;
using UnityEngine;

namespace AutoFollowCam;

[UsedImplicitly]
public class AutoFollowCam : UserMod2
{
}

public class AutoCamTimer : KMonoBehaviour
{
	internal enum TrackState
	{
		NotTracking,
		Panning,
		ManualTracking,
		AutoTracking,
	}

	private const float CamFollowTime = 30f;

	private float timeRemaining;
	internal TrackState State;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		ResetTracking();
	}

	public void ResetTracking()
	{
		timeRemaining = CamFollowTime;
		State = TrackState.NotTracking;
		CameraController.Instance.ClearFollowTarget();
	}

	private void Update()
	{
		// If the user is manually tracking something or the user is
		// panning the camera currently, don't move the timer
		if (State is TrackState.ManualTracking or TrackState.Panning)
		{
			return;
		}

		// If the player has a tool active, like the dig tool, build tool, etc
		// don't move the camera out from under them
		// SelectTool is the basic info card cursor thing, it's fine to move with that active
		if ((PlayerController.Instance.ActiveTool != null) && PlayerController.Instance.ActiveTool is not SelectTool)
		{
			return;
		}

		if ((timeRemaining > 0) && (Time.timeScale != 0))
		{
			timeRemaining -= Time.unscaledDeltaTime;
		}

		if (timeRemaining <= 0)
		{
			SwapTrackTarget();
		}
	}

	private void SwapTrackTarget()
	{
		// ReSharper disable once SimplifyLinqExpressionUseAll
		if (Components.LiveMinionIdentities.Items.Count <= 0)
		{
			return;
		}

		var targetDupe = Components.LiveMinionIdentities.Items.GetRandom();
		State = TrackState.AutoTracking;
		CameraController.Instance.SetFollowTarget(targetDupe.transform);

		timeRemaining = CamFollowTime;
	}
}

[HarmonyPatch(typeof(CameraController), "OnPrefabInit")]
public static class CameraController_OnPrefabInit_Patch
{
	[UsedImplicitly]
	public static void Postfix(CameraController __instance)
	{
		__instance.gameObject.AddOrGet<AutoCamTimer>();
	}
}

[HarmonyPatch(typeof(CameraController), "NormalCamUpdate")]
public static class CameraController_NormalCamUpdate_Patch
{
	[UsedImplicitly]
	public static void Postfix(bool ___panning, bool ___panLeft, bool ___panRight, bool ___panUp, bool ___panDown)
	{
		var autocam = CameraController.Instance.GetComponent<AutoCamTimer>();
		if (___panning || ___panLeft || ___panRight || ___panUp || ___panDown)
		{
			autocam.State = AutoCamTimer.TrackState.Panning;
		}
		else
		{
			// If the camera was previously panning, start the timer
			if (autocam.State is AutoCamTimer.TrackState.Panning)
			{
				autocam.ResetTracking();
			}
		}
	}
}

[HarmonyPatch(typeof(CameraController), "SetFollowTarget")]
public static class CameraController_SetFollowTarget_Patch
{
	[UsedImplicitly]
	public static void Postfix()
	{
		var autocam = CameraController.Instance.GetComponent<AutoCamTimer>();
		// make sure not to override the auto tracking state when we call this method
		// inside the mod to track things
		if (autocam.State != AutoCamTimer.TrackState.AutoTracking)
		{
			autocam.State = AutoCamTimer.TrackState.ManualTracking;
		}
	}
}

[HarmonyPatch(typeof(CameraController), "ClearFollowTarget")]
public static class CameraController_ClearFollowTarget_Patch
{
	[UsedImplicitly]
	public static void Postfix()
	{
		var autocam = CameraController.Instance.GetComponent<AutoCamTimer>();
		// This method gets called when selecting a target, don't reset the state if it's part of that code path 
		if (autocam.State != AutoCamTimer.TrackState.AutoTracking)
		{
			autocam.State = AutoCamTimer.TrackState.NotTracking;
		}
	}
}
