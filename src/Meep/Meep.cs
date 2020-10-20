using System;
using System.Diagnostics;
using System.Reflection;
using Database;
using Harmony;

namespace Meep
{
    [HarmonyPatch(typeof(Personalities), MethodType.Constructor, new Type[0])]
    public class Meep
    {
        public static void OnLoad()
        {
            Debug.Log(
                $"[Meep] Meep version {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}"
            );
        }

        public static void Postfix(Personalities __instance)
        {
            var meeps = __instance.resources.Count;
            var meep = __instance.resources.Find(p => p.Id == "Meep");
            for(var i = 0; i < meeps; ++i)
                __instance.resources[i] = meep;
        }
    }
}
