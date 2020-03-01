#if VERSION_1_0
using Harmony;
#else
using HarmonyLib;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

using Portal_Gun.Stances;
namespace Portal_Gun.HarmonyPatches
{
	[HarmonyPatch(typeof(Pawn_DrawTracker), "get_DrawPos")]
	static class Pawn_DrawTracker_get_DrawPos
	{
		public static void Postfix(ref Vector3 __result, Pawn ___pawn)
		{
			if (___pawn.stances != null)
			{
				Stance_UsePortal usePortal = ___pawn.stances.curStance as Stance_UsePortal;
				if (usePortal != null)
				{
					__result += usePortal.curOffset;
				}
			}
		}
	}
}
