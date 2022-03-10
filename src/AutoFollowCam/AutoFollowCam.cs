using HarmonyLib;
using KMod;
using UnityEngine;

namespace AutoFollowCam
{
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
			CameraController.Instance.SetFollowTarget(targetDupe.transform);

			State = TrackState.AutoTracking;
			timeRemaining = CamFollowTime;
		}
	}

	[HarmonyPatch(typeof(CameraController), "OnPrefabInit")]
	public static class CameraController_OnPrefabInit_Patch
	{
		public static void Postfix(CameraController __instance)
		{
			__instance.gameObject.AddOrGet<AutoCamTimer>();
		}
	}

	[HarmonyPatch(typeof(CameraController), "NormalCamUpdate")]
	public static class CameraController_NormalCamUpdate_Patch
	{
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
		public static void Postfix()
		{
			var autocam = CameraController.Instance.GetComponent<AutoCamTimer>();
			autocam.State = AutoCamTimer.TrackState.ManualTracking;
		}
	}

	[HarmonyPatch(typeof(CameraController), "ClearFollowTarget")]
	public static class CameraController_ClearFollowTarget_Patch
	{
		public static void Postfix()
		{
			var autocam = CameraController.Instance.GetComponent<AutoCamTimer>();
			autocam.State = AutoCamTimer.TrackState.NotTracking;
		}
	}
}
