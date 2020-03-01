using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Portal_Gun.Buildings;

namespace Portal_Gun
{
	static class Utilities
	{
		public static void Notify_PortalEntrySpawned(Building_PortalGunPortalEntry portalEntry)
		{
			if (!Portal_Gun.Instance.PortalManagers.ContainsKey(portalEntry.Map))
			{
				Portal_Gun.Instance.PortalManagers.Add(portalEntry.Map, new PortalManager(portalEntry.Map));
			}

			Portal_Gun.Instance.PortalManagers[portalEntry.Map].RegisterPortalEntry(portalEntry);
		}

		public static void Notify_PortalEntryDeSpawned(Building_PortalGunPortalEntry portalEntry)
		{
			if (Portal_Gun.Instance.PortalManagers[portalEntry.Map].UnregisterPortalEntry(portalEntry))
			{
				Portal_Gun.Instance.PortalManagers.Remove(portalEntry.Map);
			}
		}

		public static List<Building_PortalGunPortalEntry> GetAllPortalEntries(this Map map)
		{
			PortalManager manager;
			if (Portal_Gun.Instance.PortalManagers.TryGetValue(map, out manager))
			{
				return manager.Portals;
			}

			return null;
		}

		public static int CalculateOctileDistance(int rawDistance, Map map, int startX, int startZ, int destX, int destZ, int cardCost, int diagCost)
		{
			PortalManager manager;
			int bestCost = rawDistance;
			if (Portal_Gun.Instance.PortalManagers.TryGetValue(map, out manager))
			{
				bestCost = manager.CalculateOctileDistance(rawDistance, startX, startZ, destX, destZ, cardCost, diagCost);
			}

			return bestCost;
		}
	
	}
}
