using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Klei.AI;
using UnityEngine;

namespace BetterAnimations
{
	public static class BetterAnimationsPatches
	{
		// At level 20, should be 3.0
		// Clamp to no less than 0.75
		public static float AnimSpeed( MultitoolController.Instance instance )
		{
			var levels = instance.sm.worker.Get<AttributeLevels>( instance );
			var athletics = levels.GetAttributeLevel( "Athletics" ).level;
			return Mathf.Clamp( athletics / 6.6667f, 0.75f, float.PositiveInfinity );
		}

		private static readonly MethodInfo AnimSpeedHelper = AccessTools.Method(
			typeof(BetterAnimationsPatches),
			nameof(AnimSpeed)
		);

		[HarmonyPatch]
		public static class MultitoolController_Patches
		{
			public static IEnumerable<MethodInfo> TargetMethods()
			{
				yield return AccessTools.Method( typeof(MultitoolController.Instance), "PlayPre" );
				yield return AccessTools.Method( typeof(MultitoolController.Instance), "PlayPost" );
			}

			public static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> orig )
			{
				List<CodeInstruction> codes = orig.ToList();

				var idx = codes.FindIndex( ci => ci.opcode == OpCodes.Ldc_R4 && ci.operand is 1.0f );
				if( idx == -1 )
				{
					Debug.LogWarning( "[Better Multitool Animations] Unable to find index to patch animation speed" );
				}

				codes.RemoveAt( idx );
				codes.Insert( idx++, new CodeInstruction( OpCodes.Ldarg_0 ) );
				codes.Insert( idx, new CodeInstruction( OpCodes.Call, AnimSpeedHelper ) );

				return codes;
			}
		}
	}
}
