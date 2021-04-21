using Harmony;
using Klei.AI;
using STRINGS;

namespace BreathTraitsAffectBreathing
{
    [HarmonyPatch(typeof(SuffocationMonitor.Instance), MethodType.Constructor, typeof(OxygenBreather))]
    internal class BreathTraitPatches
    {
        public static void Postfix(ref SuffocationMonitor.Instance __instance, ref OxygenBreather oxygen_breather)
        {
            var minionIdentity = oxygen_breather.gameObject.GetComponent<MinionIdentity>();
            if(minionIdentity.GetComponent<Traits>().HasTrait("DiversLung"))
            {
                __instance.holdingbreath = new AttributeModifier(
                    Db.Get().Amounts.Breath.deltaAttribute.Id,
                    -0.6818182f,
                    DUPLICANTS.MODIFIERS.HOLDINGBREATH.NAME
                );
            }
            else if(minionIdentity.GetComponent<Traits>().HasTrait("MouthBreather"))
            {
                __instance.holdingbreath = new AttributeModifier(
                    Db.Get().Amounts.Breath.deltaAttribute.Id,
                    -1.8181818f,
                    DUPLICANTS.MODIFIERS.HOLDINGBREATH.NAME
                );
            }
            else if(minionIdentity.GetComponent<Traits>().HasTrait("DeeperDiversLungs"))
            {
                __instance.holdingbreath = new AttributeModifier(
                    Db.Get().Amounts.Breath.deltaAttribute.Id,
                    -0.454545f,
                    DUPLICANTS.MODIFIERS.HOLDINGBREATH.NAME
                );
            }
        }
    }
}
