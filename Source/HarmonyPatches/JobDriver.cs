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
	[HarmonyPatch(typeof(JobDriver), "ModifyCarriedThingDrawPos")]
	public class JobDriver_ModifyCarriedThingDrawPos
	{
		static void Postfix(JobDriver __instance, ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			if (__instance?.pawn?.equipment?.Primary is Item_PortalGun)
			{
				if (__instance.pawn.Rotation == Rot4.South)
				{
					drawPos += new Vector3(0.1f, 0.0390625f * 0.5f, -0.22f);
				}
				else if (__instance.pawn.Rotation == Rot4.North)
				{
					drawPos += new Vector3(0.1f, 0f, 0.22f);
					behind = true;
				}
				else if (__instance.pawn.Rotation == Rot4.East)
				{
					drawPos += new Vector3(0.8f, -0.0390625f * 0.5f, -0.1f);
				}
				else if (__instance.pawn.Rotation == Rot4.West)
				{
					drawPos += new Vector3(-0.8f, -0.0390625f * 0.5f, -0.1f);
				}
			}
		}
	}
}
