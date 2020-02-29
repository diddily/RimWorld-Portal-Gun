using Harmony;
using Portal_Gun.Items;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Portal_Gun.Buildings;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Portal_Gun.Harmony
{
	[HarmonyPatch(typeof(MapDrawer), "MapMeshDirty")]
	[HarmonyPatch(new Type[] { typeof(IntVec3), typeof(MapMeshFlag), typeof(bool), typeof(bool) })]
	public class MapDrawer_MapMeshDirty
	{
		static void Postfix(IntVec3 loc, MapMeshFlag dirtyFlags, bool regenAdjacentCells, bool regenAdjacentSections, Map ___map)
		{
		/*	List<Thing> things = loc.GetThingList(___map);
			bool hasPortal = things.Any(t => t is Building_PortalGunPortal || t is Building_PortalGunPortalEntry);
			if (hasPortal)
			{
				var badPortals = things.Select(t => t as Building_PortalGunPortal).Where(p => p != null).Where(p => !p.Retest()).ToList();
				foreach (var p in badPortals)
				{
					p.Destroy();
					Messages.Message(string.Format("PG_PortalCreationFailed".Translate(), p.def.LabelCap, "PG_SurfaceIncompatible_Changed".Translate()), p, MessageTypeDefOf.RejectInput, false);
				}
				var badPortals2 = things.Select(t => t as Building_PortalGunPortalEntry).Where(pe => pe != null && pe.linkedPortal != null).Select(pe => pe.linkedPortal);
				foreach (var p in badPortals2)
				{
					p.Destroy();
					Messages.Message(string.Format("PG_PortalCreationFailed".Translate(), p.def.LabelCap, "PG_SurfaceIncompatible_Changed".Translate()), p, MessageTypeDefOf.RejectInput, false);
				}
			}*/
		}
	}
}