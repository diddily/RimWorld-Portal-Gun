#if VERSION_1_0
using Harmony;
#else
using HarmonyLib;
#endif
using Portal_Gun.Items;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Portal_Gun.HarmonyPatches
{
	[HarmonyPatch(typeof(FloatMenuMakerMap), "AddDraftedOrders")]
	[HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>), typeof(bool) })]
	public class FloatMenuMakerMap_AddDraftedOrders
	{
		public static ThingWithComps get_Primary_NonPortalGun(Pawn_EquipmentTracker tracker)
		{
			ThingWithComps primary = tracker.Primary;
			if (primary is Item_PortalGun)
			{
				return null;
			}
			else
			{
				return primary;
			}
		}

		static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			bool foundCall = false;
			var instructionsList = new List<CodeInstruction>(instructions);
			for (int i = 0; i < instructionsList.Count; ++i)
			{
				if (instructionsList[i].opcode == OpCodes.Callvirt && (MethodInfo)instructionsList[i].operand == typeof(Pawn_EquipmentTracker).GetMethod("get_Primary"))
				{
					foundCall = true;
					yield return new CodeInstruction(OpCodes.Call, typeof(FloatMenuMakerMap_AddDraftedOrders).GetMethod("get_Primary_NonPortalGun"));
				}
				else
				{
					yield return instructionsList[i];
				}
			}
			if (!foundCall)
			{
				Log.Warning("Failed to patch FloatMenuMakerMap.AddDraftedOrders, couldn't find: callvirt instance class Verse.ThingWithComps Verse.Pawn_EquipmentTracker::get_Primary()");
			}
		}
	}
}