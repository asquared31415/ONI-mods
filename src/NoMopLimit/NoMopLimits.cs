using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;

namespace NoMopLimit
{
    public static class NoMopLimit
    {
        public static void OnLoad() { MopTool.maxMopAmt = float.PositiveInfinity; }
    }

    [HarmonyPatch(typeof(MopTool), "OnDragTool")]
    public static class MopTool_OnDragTool_Patch
    {
        public static readonly MethodInfo CellBelowInfo = AccessTools.Method(typeof(Grid), "CellBelow");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> orig)
        {
            var codes = orig.ToList();
            var idx = codes.FindIndex(ci => ci.operand is MethodInfo i && i == CellBelowInfo);
            if(idx != -1)
            {
                var i = idx - 2;
                // Remove all before store to flag
                codes.RemoveRange(i, 4);
                codes.Insert(i, new CodeInstruction(OpCodes.Ldc_I4_1));
            }

            return codes;
        }
    }
}
