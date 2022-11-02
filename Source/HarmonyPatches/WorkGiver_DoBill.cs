using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Portal_Gun.HarmonyPatches
{

	[HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
	internal class WorkGiver_DoBill_TryFindBestBillIngredients
	{
		private static void Postfix(WorkGiver_DoBill __instance, Bill bill, Pawn pawn, ref List<ThingCount> chosen, ref bool __result)
		{
			if (bill.recipe == PG_DefOf.PG_Craft_VanometricPowerCell && !__result)
			{
				Thing thing = pawn.Map.spawnedThings.FirstOrDefault((Thing t) => t.GetInnerIfMinified().def == ThingDefOf.VanometricPowerCell && bill.IsFixedOrAllowedIngredient(t) && pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly) && !t.IsForbidden(pawn) && t is MinifiedThing);
				if (thing != null)
				{
					ThingCountUtility.AddToList(chosen, thing, 1);
					__result = true;
				}
			}
		}
	}
}