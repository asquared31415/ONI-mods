using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using TUNING;

namespace FartFrequently
{
	public class FartFrequently
	{
		public static readonly ConfigReader Conf = new ConfigReader();
		public static readonly FileSystemWatcher Watcher = new FileSystemWatcher();

		private static void OnChanged(object source, FileSystemEventArgs a)
		{
			Flatulence_Emit_Patch.SetValues();
		}

		public static void OnLoad()
		{
			Watcher.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			Watcher.NotifyFilter = NotifyFilters.LastWrite;

			// Add event handlers.
			Watcher.Changed += OnChanged;

			// Begin watching.
			Watcher.EnableRaisingEvents = true;
			Flatulence_Emit_Patch.SetValues();
		}
	}

	[HarmonyPatch(typeof(Flatulence), "Emit")]
	public class Flatulence_Emit_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = instructions.ToList();

			for (var i = 0; i < codes.Count; i++)
			{
				var code = codes[i];
				if (code.operand is int val)
				{
					if (val == (int) SimHashes.Methane)
					{
						codes.RemoveAt(i);
						codes.Insert(
							i++,
							new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FartFrequently), "Conf"))
						);

						codes.Insert(
							i++,
							new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ConfigReader), "ElementHash"))
						);

						var idx = codes.FindIndex(i, ci => ci.operand is float v && v == 0.1f);
						codes[idx++] = new CodeInstruction(
							OpCodes.Ldsfld,
							AccessTools.Field(typeof(FartFrequently), "Conf")
						);

						codes.Insert(
							idx,
							new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ConfigReader), "EmitAmount"))
						);
					}
				}
			}

			return codes;
		}

		public static void SetValues()
		{
			FartFrequently.Conf.SetFromConfig();
			TRAITS.FLATULENCE_EMIT_INTERVAL_MIN = FartFrequently.Conf.Min;
			TRAITS.FLATULENCE_EMIT_INTERVAL_MAX = FartFrequently.Conf.Max;

			Debug.Log(
				"[FartFrequently]: (Config Loader) The farting config has been changed to emit " +
				FartFrequently.Conf.EmitAmount + "Kg of " + FartFrequently.Conf.Element + " at a " +
				FartFrequently.Conf.Min + "-" + FartFrequently.Conf.Max + " interval"
			);
		}
	}
}
