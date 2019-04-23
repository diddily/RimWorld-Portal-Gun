using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Buildings;

namespace Portal_Gun.Harmony
{
    [HarmonyPatch(typeof(GenAdj), "AdjacentTo8WayOrInside")]
    [HarmonyPatch(new Type[] { typeof(IntVec3), typeof(Thing) })]
    static class GenAdj_AdjacentTo8WayOrInside
    {
        public static void Postfix(ref bool __result, IntVec3 root, Thing t)
        {
            if (!__result)
            {
                __result = root.GetThingList(t.Map).OfType<Building_PortalGunPortalEntry>().Where(pe => pe.IsConnected).Select(pe => pe.Exit.Position).Any(p => p.AdjacentTo8WayOrInside(t.Position, t.Rotation, t.def.size));
                if (__result)
                {
                    Log.Message("AdjacentTo8WayOrInside (" + root + " -> " + t + ") changed result");
                }
            }
        }
    }
}
