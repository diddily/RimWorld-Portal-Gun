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
	[HarmonyPatch(typeof(PawnRenderUtility), "CarryWeaponOpenly")]
	class PawnRenderUtility_CarryWeaponOpenly
	{
		static void Postfix(ref bool __result, ref Pawn pawn)
		{
			if (!__result && pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
			{
				if (pawn.equipment.Primary is Item_PortalGun)
				{
					Vector3 dummy = Vector3.zero;
					bool dummy1 = false;
					if (pawn.CurJob == null || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref dummy, ref dummy1))
					{
						__result = true;
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
	class PawnRenderUtility_DrawEquipmentAiming
	{
		static bool Prefix(Thing eq, Vector3 drawLoc, ref float aimAngle)
		{
			Item_PortalGun portal_gun = eq as Item_PortalGun;
			if (eq is Item_PortalGun && portal_gun.holder.carryTracker != null && portal_gun.holder.carryTracker.CarriedThing != null)
			{
				Vector3 dummy = Vector3.zero;
				bool dummy1 = false;
				if (portal_gun.holder.CurJob == null || !portal_gun.holder.jobs.curDriver.ModifyCarriedThingDrawPos(ref dummy, ref dummy1))
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

	[HarmonyPatch(typeof(PawnRenderNodeWorker_Carried), "PostDraw")]
	class PawnRenderNodeWorker_Carried_PostDraw
	{
		static void Postfix(ref PawnRenderNodeWorker_Carried __instance, PawnRenderNode node, PawnDrawParms parms, Mesh mesh, Matrix4x4 matrix)
		{
			if (parms.pawn.carryTracker?.CarriedThing != null)
			{
				if (parms.pawn.equipment.Primary is Item_PortalGun)
				{
					Vector3 pivot;
					Vector3 vector = parms.matrix.Position() + __instance.OffsetFor(node, parms, out pivot);
					PawnRenderUtility.DrawEquipmentAndApparelExtras(parms.pawn, vector, parms.facing, parms.flags);
				}
			}
		}
	}
}
