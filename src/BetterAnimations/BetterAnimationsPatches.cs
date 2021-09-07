using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Klei.AI;
using KMod;
using UnityEngine;

namespace BetterAnimations
{
	public class BetterAnimationsInfo : UserMod2
	{
	}

	public static class BetterAnimationsPatches
	{
		[HarmonyPatch]
		public static class MultitoolController_Patch
		{
			private static readonly MethodInfo PlayMethodInfo = AccessTools.Method(
				typeof(KAnimControllerBase),
				"Play",
				new[]
				{
					typeof(HashedString),
					typeof(KAnim.PlayMode),
					typeof(float), typeof(float),
				}
			);

			private static readonly MethodInfo SpeedHelper = AccessTools.Method(
				typeof(MultitoolController_Patch),
				nameof(GetScaleHelper)
			);

			public static IEnumerable<MethodInfo> TargetMethods()
			{
				yield return AccessTools.Method(typeof(MultitoolController.Instance), "PlayPre");
				yield return AccessTools.Method(typeof(MultitoolController.Instance), "PlayPost");
			}

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
			{
				var codes = orig.ToList();
				var idx = codes.FindIndex(ci => ci.operand is MethodInfo m && m == PlayMethodInfo);
				if (idx != -1)
				{
					--idx;
					// Insert after the load of the speed mult
					codes.Insert(idx++, new CodeInstruction(OpCodes.Ldarg_0));
					codes.Insert(idx, new CodeInstruction(OpCodes.Call, SpeedHelper));
				}
				else
				{
					Debug.LogError("Could not patch multitool animation speed");
				}

				return codes;
			}

			// At level 20, should be 3.0
			// Clamp to no less than 0.75
			private static float GetScaleHelper(float scale, MultitoolController.Instance smi)
			{
				var levels = smi.sm.worker.Get<AttributeLevels>(smi);
				if (levels == null)
				{
					// failsafe to 1.0x if the worker doesn't have any attributes (impossible in Klei code, used for compatibility)
					return 1f;
				}

				var athletics = levels.GetAttributeLevel("Athletics");
				if (athletics == null)
				{
					// failsafe to 1.0x if the worker doesn't have `Athletics` specifically (impossible in Klei code, used for compatibility)
					return 1f;
				}
				return scale * Mathf.Clamp(athletics.level / 6.6667f, 0.75f, float.PositiveInfinity);
			}
		}
	}
}
