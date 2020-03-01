#if VERSION_1_0
using Harmony;
#else
using HarmonyLib;
#endif
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
namespace Portal_Gun.HarmonyPatches
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

	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
	class PawnRenderer_DrawEquipmentAiming
	{
		static bool Prefix(Pawn ___pawn, Thing eq, Vector3 drawLoc, ref float aimAngle)
		{
			if (eq is Item_PortalGun && ___pawn.carryTracker != null && ___pawn.carryTracker.CarriedThing != null)
			{
				Vector3 dummy = Vector3.zero;
				bool dummy1 = false;
				bool dummy2 = false;
				if (___pawn.CurJob == null || !___pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref dummy, ref dummy1, ref dummy2))
				{
					if (aimAngle == 143f)
					{
						aimAngle = 90f;
					}
					else if (aimAngle == 217f)
					{
						aimAngle = 270f;
					}
				}
			}
			return true;
		}
	}
}
