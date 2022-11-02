using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
#if VERSION_1_0
using Harmony;
#else
using HarmonyLib;
#endif
using System.Reflection;
using Portal_Gun.Buildings;
using Portal_Gun.Items;
using UnityEngine;

namespace Portal_Gun.HarmonyPatches
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			//Harmony
#if VERSION_1_0
			HarmonyInstance harmony = HarmonyInstance.Create("diddily.Portal_Gun");
#else
			Harmony harmony = new Harmony("diddily.Portal_Gun");
#endif
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

			{
				Type type = AccessTools.TypeByName("VFESecurity.StatPart_AmmoCrate");
				if (type != null)
				{
					Log.Message("Patching VFESecurity.StatPart_AmmoCrate.CanAffect.");
					MethodInfo originalMethod = AccessTools.Method(type, "CanAffect");
					HarmonyMethod patchMethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_CanAffect)));
					harmony.Patch(
						originalMethod,
						patchMethod,
						null);
				}
			}

		}

		public static bool Patch_TryGetAttackVerb(Pawn __instance, Thing target, bool allowManualCastWeapons)
		{
			if (__instance.equipment != null && __instance.equipment.Primary != null && __instance.equipment.Primary is Item_PortalGun)
			{
				return ((Item_PortalGun)__instance.equipment.Primary).AllowsViolence;
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
		public static bool Patch_PostLoad(ThingDef __instance)
		{
			Log.Error(__instance.defName);
			return true;
		}
		public static void Patch_BuildingBlockingNextPathCell(ref Building __result, ref Pawn_PathFollower __instance, ref Pawn ___pawn)
		{
			if (__result != null && __instance.curPath != null && __instance.curPath.NodesLeftCount > 1)
			{
				Pawn_PathFollower instance = __instance;
				Pawn pawn = ___pawn;
				Building_PortalGunPortalEntry portal1 = instance.nextCell.GetThingList(pawn.Map).OfType<Building_PortalGunPortalEntry>().Where(p => p.IsConnected && p.Exit.Position == instance.curPath.Peek(1)).FirstOrDefault();
				if (portal1 != null)
				{
					__result = null;
				}
			}
		}

		public static bool Patch_CanAffect(ref bool __result, StatRequest req)
		{
			if (req.Thing is Item_PortalGun)
			{
				__result = false;
				return false;
			}
			return true;
		}
	}
}
 