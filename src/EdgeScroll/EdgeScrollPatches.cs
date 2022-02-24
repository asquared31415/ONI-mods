using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;
using Newtonsoft.Json;
using UnityEngine;

namespace EdgeScroll
{
	public class EdgeScrollInfo : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			harmony.PatchAll();

			// load config early
			var _ = EdgeScrollConfig.Instance;
		}
	}

	public class EdgeScrollConfig
	{
		public static readonly string ModPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName;
		public const string ConfigName = "config.json";
		public static readonly string ConfigPath = Path.Combine(ModPath, ConfigName);

		public class Config
		{
			public int BorderSize = 15;
			public float PanSpeed = 1;
		}

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly FileSystemWatcher watcher;

		public static EdgeScrollConfig Instance { get; } = new();

		public Config Data;

		private EdgeScrollConfig()
		{
			if (!File.Exists(ConfigPath))
			{
				var config = new Config();
				File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config));
			}

			Load();

			watcher = new FileSystemWatcher
			{
				Path = ModPath,
				Filter = ConfigName,
				NotifyFilter = NotifyFilters.LastWrite,
			};
			watcher.Changed += (_, _) => Load();
			watcher.EnableRaisingEvents = true;
		}

		private void Load()
		{
			try
			{
				var text = File.ReadAllText(ConfigPath);
				Data = JsonConvert.DeserializeObject<Config>(text);

				if (CameraController.Instance != null)
				{
					CameraController.Instance.keyPanningSpeed = Data.PanSpeed;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Edge Scroll] Unable to read config file, using defaults");
				Debug.LogWarning($"\tReason: {e.Message}");
				Data = new Config();
			}
		}
	}

	[Flags]
	public enum EnabledMouseDirections
	{
		None = 0b0000,
		Left = 0b0001,
		Right = 0b0010,
		Up = 0b0100,
		Down = 0b1000,
	}

	[HarmonyPatch(typeof(CameraController), "OnPrefabInit")]
	public static class CameraControllerOnPrefabInitPatch
	{
		public static void Postfix(CameraController __instance)
		{
			__instance.keyPanningSpeed = EdgeScrollConfig.Instance.Data.PanSpeed;
		}
	}

	[HarmonyPatch(typeof(CameraController), "Update")]
	public static class CameraControllerUpdatePatch
	{
		private static EnabledMouseDirections prevDirections = EnabledMouseDirections.None;

		public static void Prefix(
			ref bool ___panLeft,
			ref bool ___panRight,
			ref bool ___panUp,
			ref bool ___panDown
		)
		{
			if (!Application.isFocused)
			{
				return;
			}

			var flags = EnabledMouseDirections.None;
			var borderSize = EdgeScrollConfig.Instance.Data.BorderSize;

			var pos = KInputManager.GetMousePos();
			var isOutside = (pos.x < 0.0) || (pos.x >= Screen.width) || (pos.y < 0.0) || (pos.y >= Screen.height);

			if (!isOutside)
			{
				if (pos.x <= borderSize)
				{
					flags |= EnabledMouseDirections.Left;
				}
				else if (Screen.width - borderSize <= pos.x)
				{
					flags |= EnabledMouseDirections.Right;
				}

				if (pos.y <= borderSize)
				{
					flags |= EnabledMouseDirections.Down;
				}
				else if (Screen.height - borderSize <= pos.y)
				{
					flags |= EnabledMouseDirections.Up;
				}
			}

			if (flags.HasFlag(EnabledMouseDirections.Left))
			{
				___panLeft = true;
			}
			else if (prevDirections.HasFlag(EnabledMouseDirections.Left))
			{
				___panLeft = false;
			}

			if (flags.HasFlag(EnabledMouseDirections.Right))
			{
				___panRight = true;
			}
			else if (prevDirections.HasFlag(EnabledMouseDirections.Right))
			{
				___panRight = false;
			}

			if (flags.HasFlag(EnabledMouseDirections.Up))
			{
				___panUp = true;
			}
			else if (prevDirections.HasFlag(EnabledMouseDirections.Up))
			{
				___panUp = false;
			}

			if (flags.HasFlag(EnabledMouseDirections.Down))
			{
				___panDown = true;
			}
			else if (prevDirections.HasFlag(EnabledMouseDirections.Down))
			{
				___panDown = false;
			}

			prevDirections = flags;
		}
	}
}
