﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;
using System.Reflection;
using Portal_Gun.Buildings;
using Portal_Gun.Items;
using UnityEngine;

namespace Portal_Gun
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //Harmony
            HarmonyInstance harmony = HarmonyInstance.Create("diddily.Portal_Gun");

            {
                Type type = typeof(Verse.CompEquippable);
                MethodInfo originalMethod = AccessTools.Method(type, "GetVerbsCommands");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_GetVerbsCommands)));
                harmony.Patch(
                    originalMethod,
                    null,
                    patchMethod,
                    null);
            }

            {
                Type type = typeof(Verse.Pawn);
                MethodInfo originalMethod = AccessTools.Method(type, "TryGetAttackVerb");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_TryGetAttackVerb)));
                harmony.Patch(
                    originalMethod,
                    patchMethod,
                    null);
            }

            {
                Type type = typeof(Verse.ThingDef);
                MethodInfo originalMethod = AccessTools.Method(type, "get_AffectsRegions");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_get_AffectsRegions)));
                harmony.Patch(
                    originalMethod,
                    null,
                    patchMethod,
                    null);
            }
            
            {
                Type type = typeof(Verse.AI.Pawn_PathFollower);
                MethodInfo originalMethod = AccessTools.Method(type, "BuildingBlockingNextPathCell");
                HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_BuildingBlockingNextPathCell)));
                harmony.Patch(
                    originalMethod,
                    null,
                    patchMethod,
                    null);
            }

        }

        public static void Patch_GetVerbsCommands(ref IEnumerable<Command> __result, Verse.CompEquippable __instance)
        {
            //if (__instance.parent is Item_PortalGun)
            //{
            //    __result = __result.Concat(((Item_PortalGun)__instance.parent).GetCommands());
            //}
        }

        public static bool Patch_TryGetAttackVerb(Pawn __instance, Thing target, bool allowManualCastWeapons)
        {
            if (__instance.equipment != null && __instance.equipment.Primary != null && __instance.equipment.Primary is Item_PortalGun)
            {
                return ((Item_PortalGun)__instance.equipment.Primary).HasViolenceCoreModule;
            }

            return true;
        }

        public static void Patch_get_AffectsRegions(ref bool __result, ThingDef __instance)
        {
            if (__instance == PG_DefOf.PG_PortalEntry)
            {
                __result = true;
            }
        }

        public static void Patch_BuildingBlockingNextPathCell(ref Building __result, ref Pawn_PathFollower __instance, ref Pawn ___pawn)
        {
            if (__result != null && __instance.curPath.NodesLeftCount > 1)
            {
                Pawn_PathFollower instance = __instance;
                Pawn pawn = ___pawn;
                Building_PortalGunPortal portal1 = instance.nextCell.GetThingList(pawn.Map).OfType<Building_PortalGunPortal>().Where(p => p.LinkedPortal.Position == instance.curPath.Peek(1)).FirstOrDefault();
                if (portal1 != null)
                {
                    __result = null;
                }
            }
        }
    }
}
 