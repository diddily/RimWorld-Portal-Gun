#if VERSION_1_0
using Harmony;
#else
using HarmonyLib;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Items;
using Portal_Gun.Projectiles;

namespace Portal_Gun.HarmonyPatches
{
	[HarmonyPatch(typeof(Projectile), "Launch")]
	[HarmonyPatch(new Type[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
	public class Projectile_Launch
	{
		public static void Postfix(ref Projectile __instance, Thing equipment)
		{
			Projectile_PortalGun projectile = __instance as Projectile_PortalGun;
			Item_PortalGun portalGun = equipment as Item_PortalGun;
			if (projectile != null && portalGun != null)
			{
				projectile.launcherGun = portalGun;
				portalGun.isSecondary = projectile.def == PG_DefOf.PG_PortalGun_BulletSecondary;
			}
		}
	}
}
