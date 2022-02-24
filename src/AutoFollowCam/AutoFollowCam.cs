using System.Linq;
using HarmonyLib;
using KMod;

namespace AutoFollowCam
{
	public class AutoFollowCam : UserMod2
	{
	}

	public class AutoCamTimer : KMonoBehaviour, ISim1000ms
	{
		private const float CamFollowTime = 30f;

		private float timeRemaining;
		private bool isTracking;
		public bool panning;

		protected override void OnSpawn()
		{
			base.OnSpawn();
			timeRemaining = CamFollowTime;
			isTracking = false;
			panning = false;
		}

		public void ResetTimer()
		{
			timeRemaining = CamFollowTime;
			isTracking = false;
			CameraController.Instance.ClearFollowTarget();
		}

		public void Sim1000ms(float dt)
		{
			if (isTracking || panning)
			{
				return;
			}

			if (timeRemaining > 0)
			{
				timeRemaining -= dt;
			}

			if (timeRemaining <= 0)
			{
				StartTracking();
			}
		}

		private void StartTracking()
		{
			isTracking = true;
			// ReSharper disable once SimplifyLinqExpressionUseAll
			if (!Components.LiveMinionIdentities.Items.Any(
					e => e.GetMyWorldId() == ClusterManager.Instance.activeWorldId
				))
			{
				return;
			}

			var targetDupe = Components.LiveMinionIdentities.Items
				.Where(e => e.GetMyWorldId() == ClusterManager.Instance.activeWorldId)
				.ToList()
				.GetRandom();
			CameraController.Instance.SetFollowTarget(targetDupe.transform);
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
				autocam.panning = true;
				autocam.ResetTimer();
			}
			else
			{
				// if it was panning, reset the timer
				if (autocam.panning)
				{
					autocam.ResetTimer();
				}

				autocam.panning = false;
			}
		}
	}
}
