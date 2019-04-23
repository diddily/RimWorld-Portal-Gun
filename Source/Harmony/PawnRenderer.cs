using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Buildings;
using Portal_Gun.Items;
using Portal_Gun.Jobs;
namespace Portal_Gun.Harmony
{
    [HarmonyPatch(typeof(PawnRenderer), "CarryWeaponOpenly")]
    class PawnRenderer_CarryWeaponOpenly
    {
        static void Postfix(ref bool __result, PawnRenderer __instance, Pawn ___pawn)
        {
            if (!__result && ___pawn.carryTracker != null && ___pawn.carryTracker.CarriedThing != null)
            {
                if (___pawn.equipment.Primary is Item_PortalGun)
                {
                    Vector3 dummy = Vector3.zero;
                    bool dummy1 = false;
                    bool dummy2 = false;
                    if (___pawn.CurJob == null || !___pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref dummy, ref dummy1, ref dummy2))
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}
