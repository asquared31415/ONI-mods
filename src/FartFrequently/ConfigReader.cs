using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace FartFrequently
{
    public static class EnumExt
    {
        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, IConvertible
        {
            var retValue = value != null && Enum.IsDefined(typeof(TEnum), value);
            result = retValue ? (TEnum) Enum.Parse(typeof(TEnum), value) : default;
            return retValue;
        }
    }

    public class ConfigReader
    {
        private static readonly string Path =
            System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config.json";

        public string Element;
        public SimHashes ElementHash;
        public float EmitAmount;
        public float Max;
        public float Min;

        public ConfigReader()
        {
            Min = 10f;
            Max = 40f;
            EmitAmount = 0.1f;
            Element = "Methane";
            ElementHash = SimHashes.Methane;
        }

        public void SetFromConfig()
        {
            try
            {
                if(!File.Exists(Path))
                {
                    using var fs = File.Create(Path);
                    var text = new UTF8Encoding(true).GetBytes(JsonConvert.SerializeObject(this));
                    fs.Write(text, 0, text.Length);
                }
                else
                {
                    string json;
                    using(var sr = new StreamReader(Path))
                    {
                        json = sr.ReadToEnd();
                    }

                    var newConf = JsonConvert.DeserializeObject<ConfigReader>(json);
                    Min = newConf.Min;
                    Max = newConf.Max;
                    EmitAmount = newConf.EmitAmount;
                    Element = newConf.Element;
                    if(Min > Max)
                    {
                        Debug.Log(
                            "[FartFrequently]: (Config Loader) The minimum value is greater than the maximum, this may cause strange behavior"
                        );
                    }

                    if(EmitAmount <= 0)
                    {
                        EmitAmount = 0.1f;
                        Debug.Log(
                            "[FartFrequently]: (Config Loader) The emit amount is set to a negative or zero value, resetting to 0.1"
                        );
                    }

                    if(!EnumExt.TryParse(Element, out SimHashes elementEnum))
                    {
                        Debug.LogWarning($"[FartFrequently] {Element} is not a SimHashes element");
                        elementEnum = SimHashes.Methane;
                    }

                    ElementHash = elementEnum;
                    Debug.Log(ElementHash);
                }
            }
            catch
            {
                Min = 10f;
                Max = 40f;
                EmitAmount = 0.1f;
                Element = "Methane";
                ElementHash = SimHashes.Methane;
                Debug.Log(
                    "[FartFrequently]: (Config Loader) An error occured, please ensure you are using only numerical values in the config file"
                );
            }
        }
    }
}
