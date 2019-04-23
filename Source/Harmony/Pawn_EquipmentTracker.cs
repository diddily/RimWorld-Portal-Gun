using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Items;

namespace Portal_Gun.Harmony
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    public class Pawn_EquipmentTracker_Notify_EquipmentAdded
    {
        static void Postfix(ref Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            Item_PortalGun portalGun = eq as Item_PortalGun;
            if (portalGun != null)
            {
                foreach (Verb current in portalGun.Verbs)
                {
                    current.caster = __instance.pawn;
                    current.Notify_PickedUp();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentRemoved")]
    public class Pawn_EquipmentTracker_Notify_EquipmentRemoved
    {
        static void Postfix(ThingWithComps eq)
        {
            Item_PortalGun portalGun = eq as Item_PortalGun;
            if (portalGun != null)
            {
                foreach (Verb current in portalGun.Verbs)
                {
                    current.Notify_EquipmentLost();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
    public class Pawn_EquipmentTracker_GetGizmos
    {
        static void Postfix(ref IEnumerable<Gizmo> __result, ref Pawn_EquipmentTracker __instance)
        {
            if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
            {
                List<ThingWithComps> list = __instance.AllEquipmentListForReading;
                for (int i = 0; i < list.Count; i++)
                {
                    Item_PortalGun portalGun = list[i] as Item_PortalGun;
                    if (portalGun != null)
                    {
                        __result = __result.Concat(portalGun.GetGizmos());
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick")]
    public class Pawn_EquipmentTracker_EquipmentTrackerTick
    {
        static void Postfix(ref Pawn_EquipmentTracker __instance)
        {
            List<ThingWithComps> list = __instance.AllEquipmentListForReading;
            for (int i = 0; i < list.Count; i++)
            {
                Item_PortalGun portalGun = list[i] as Item_PortalGun;
                if (portalGun != null)
                {
                    portalGun.Tick();
                }
            }
        }
    }
}