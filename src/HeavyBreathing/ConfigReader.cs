using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace HeavyBreathing
{
	public class ConfigReader
	{
		private static readonly string Path
			= System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config.json";

		public float EmitAmount;

		public ConfigReader()
		{
			EmitAmount = 0.02f;
		}

		public void SetFromConfig()
		{
			try
			{
				if (!File.Exists(Path))
				{
					using var fs = File.Create(Path);
					var text = new UTF8Encoding(true).GetBytes(JsonConvert.SerializeObject(this));
					fs.Write(text, 0, text.Length);
				}
				else
				{
					string json;
					using (var sr = new StreamReader(Path))
					{
						json = sr.ReadToEnd();
					}

					var newConf = JsonConvert.DeserializeObject<ConfigReader>(json);
					EmitAmount = newConf.EmitAmount;
					if (EmitAmount <= 0)
					{
						EmitAmount = 0.02f;
						Debug.Log(
							"[Heavy Breathing]: (Config Loader) The emit amount is set to a negative or zero value, resetting to 0.02 Kg"
						);
					}
				}
			}
			catch (Exception e) when (e.GetType() == typeof(IOException) ||
			                          e.GetType() == typeof(UnauthorizedAccessException))
			{
				EmitAmount = 0.002f;
				Debug.Log(
					"[Heavy Breathing]: (Config Loader) An error occured, please ensure you are using only numerical values in the config file"
				);
			}
		}
	}
}
