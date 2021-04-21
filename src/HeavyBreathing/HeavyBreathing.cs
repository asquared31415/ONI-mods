using System.IO;
using System.Reflection;
using Harmony;

namespace HeavyBreathing
{
    class HeavyBreathing
    {
        public static void OnLoad()
        {
            Watcher.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Watcher.NotifyFilter = NotifyFilters.LastWrite;

            // Add event handlers.
            Watcher.Changed += OnChanged;

            // Begin watching.
            Watcher.EnableRaisingEvents = true;
            CO2Manager_SpawnBreath_Patch.SetValues();
        }

        public static readonly ConfigReader Conf = new ConfigReader();
        private static readonly FileSystemWatcher Watcher = new FileSystemWatcher();

        private static void OnChanged(object source, FileSystemEventArgs a)
        {
            CO2Manager_SpawnBreath_Patch.SetValues();
        }
    }

    [HarmonyPatch(typeof(CO2Manager), "SpawnBreath")]
    public class CO2Manager_SpawnBreath_Patch
    {
        private static float emitAmount = 0.02f;

        public static void Prefix(ref float mass) { mass = emitAmount; }

        public static void SetValues()
        {
            HeavyBreathing.Conf.SetFromConfig();
            emitAmount = HeavyBreathing.Conf.EmitAmount;
            Debug.Log(
                "[Heavy Breathing]: (Config Loader) The emit amount has been changed to " +
                HeavyBreathing.Conf.EmitAmount +
                "Kg"
            );
        }
    }
}
