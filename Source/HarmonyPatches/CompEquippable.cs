using System.Collections.Generic;
using HarmonyLib;
using Portal_Gun.Items;
using Verse;

namespace Portal_Gun.HarmonyPatches
{
	[HarmonyPatch(typeof(CompEquippable), "Verse.IVerbOwner.get_ConstantCaster")]
	public class CompEquippable_GetConstantCaster
	{
		public static bool Prefix(ref Thing __result, ref CompEquippable __instance)
		{
			if (__instance.parent is Item_PortalGun item_PortalGun)
			{
				__result = item_PortalGun.SpawnedParentOrMe;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CompEquippable), "get_Holder")]
	public class CompEquippable_GetHolder
	{
		public static bool Prefix(ref Pawn __result, ref CompEquippable __instance)
		{
			if (__instance.PrimaryVerb == null)
			{
				__result = null;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CompEquippable), "get_VerbProperties")]
	public class CompEquippable_GetVerbProperties
	{
		public static bool Prefix(ref List<VerbProperties> __result, ref CompEquippable __instance)
		{
			if (__instance.parent is Item_PortalGun item_PortalGun)
			{
				__result = item_PortalGun.VerbProperties;
				return false;
			}
			return true;
		}
	}
}