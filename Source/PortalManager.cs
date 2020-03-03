using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using Portal_Gun.Buildings;

namespace Portal_Gun
{
	public class PortalManager
	{
		public static int DefaultCardinalCost = 13;
		public static int DefaultDiagonalCost = 18;
		public Map Map { get; private set; }
		public List<Building_PortalGunPortalEntry> Portals { get; private set; }

		private Dictionary<Building_PortalGunPortalEntry, PortalPath> cachedDestCosts;
		private int cachedDestX;
		private int cachedDestZ;
		private Dictionary<Building_PortalGunPortalEntry, Dictionary<Building_PortalGunPortalEntry, PortalPath>> DistanceLookup;
		private bool dirty;

		private class PortalPath
		{
			public PortalPath(int dx, int dz, int count)
			{
				Count = count;
				X = dx;
				Z = dz;
			}
			int Count { get; set; }
			int X { get; set; }
			int Z { get; set; }

			public static int GetCost(int dx, int dz, int count, int cardinalCost, int diagonalCost)
			{
				return GenMath.OctileDistance(dx, dz, cardinalCost, diagonalCost) + cardinalCost * count;
			}

			public int GetCost(int cardinalCost, int diagonalCost)
			{
				return GetCost(X, Z, Count, cardinalCost, diagonalCost);
			}

			public override string ToString()
			{
				return "(" + X + "," + Z + "," + Count + ")";
			}
		}

		public PortalManager(Map map)
		{
			Map = map;
			Portals = new List<Building_PortalGunPortalEntry>();
			DistanceLookup = null;
			dirty = true;
		}

		public void RegisterPortalEntry(Building_PortalGunPortalEntry portalEntry)
		{
			if (portalEntry.Map != Map)
			{
				Log.Error("Map mismatch!");
			}
			else
			{
				Portals.Add(portalEntry);
				dirty = true;
			}
		}
		public bool UnregisterPortalEntry(Building_PortalGunPortalEntry portalEntry)
		{
			if (portalEntry.Map != Map)
			{
				Log.Error("Map mismatch!");
			}
			else
			{
				Portals.Remove(portalEntry);
				dirty = true;
			}

			return Portals.Count == 0;
		}

		private void UpdateDirtyRecursive(ref PortalPath bestPath, Building_PortalGunPortalEntry start, Building_PortalGunPortalEntry target, int pdx, int pdz, Stack<Building_PortalGunPortalEntry> visited)
		{
		   
			Building_PortalGunPortalEntry startExit = start.Exit;
			visited.Push(start);
			visited.Push(startExit);
			int dx = pdx + Math.Abs(startExit.Position.x - target.Position.x);
			int dz = pdz + Math.Abs(startExit.Position.z - target.Position.z);
			int count = visited.Count / 2;
			if (bestPath == null || PortalPath.GetCost(dx, dz, count, DefaultCardinalCost, DefaultDiagonalCost) < bestPath.GetCost(DefaultCardinalCost, DefaultDiagonalCost))
			{
				bestPath = new PortalPath(dx, dz, count);
			}
			foreach (Building_PortalGunPortalEntry test in Portals.Where(pe => !visited.Contains(pe)))
			{
				int tdx = pdx + Math.Abs(startExit.Position.x - test.Position.x);
				int tdz = pdz + Math.Abs(startExit.Position.z - test.Position.z);
				if (PortalPath.GetCost(tdx, tdz, count+1, DefaultCardinalCost, DefaultDiagonalCost) < bestPath.GetCost(DefaultCardinalCost, DefaultDiagonalCost))
				{
					UpdateDirtyRecursive(ref bestPath, test, target, tdx, tdz, visited);
				}
			}
			visited.Pop();
			visited.Pop();
		}

		private void UpdateDirty()
		{
			DistanceLookup = new Dictionary<Building_PortalGunPortalEntry, Dictionary<Building_PortalGunPortalEntry, PortalPath>>(Portals.Count);
			Stack<Building_PortalGunPortalEntry> visited = new Stack<Building_PortalGunPortalEntry>(Portals.Count);
		 
			foreach (Building_PortalGunPortalEntry start in Portals)
			{
				Building_PortalGunPortalEntry startExit = start.Exit;
				DistanceLookup[start] = new Dictionary<Building_PortalGunPortalEntry, PortalPath>(Portals.Count - 1);
				// Special case for our own linked portal
				DistanceLookup[start][startExit] = new PortalPath(0, 0, 1);
				foreach (Building_PortalGunPortalEntry end in Portals.Where(pe => pe != start && pe != startExit))
				{
					Building_PortalGunPortalEntry endExit = end.Exit;
					PortalPath bestPath = null;
					visited.Push(end);
					visited.Push(endExit);
					UpdateDirtyRecursive(ref bestPath, start, endExit, 0, 0, visited);
					DistanceLookup[start][end] = bestPath;
					visited.Pop();
					visited.Pop();
				}
			}
			dirty = false;
		}

		public int CalculateOctileDistance(int bestCost, int startX, int startZ, int destX, int destZ, int cardCost, int diagCost)
		{
			if (dirty)
			{
				UpdateDirty();
				cachedDestX = -1;
				cachedDestZ = -1;

				foreach (Building_PortalGunPortalEntry entry in Portals)
				{
					foreach (Building_PortalGunPortalEntry exit in Portals)
					{
						if (entry == exit)
						{
							continue;
						}
						Portal_Gun.Message(entry.Position + " to " + exit.Position + " path is " + DistanceLookup[entry][exit] + "("+ DistanceLookup[entry][exit].GetCost(DefaultCardinalCost, DefaultDiagonalCost)+")");
					}
				}
			}
			if (cachedDestX != destX || cachedDestZ != destZ)
			{
				cachedDestCosts = new Dictionary<Building_PortalGunPortalEntry, PortalPath>(Portals.Count);
				foreach (Building_PortalGunPortalEntry pe in Portals)
				{
					int dx = Math.Abs(pe.Position.x - destX);
					int dz = Math.Abs(pe.Position.z - destZ);
					cachedDestCosts[pe] = new PortalPath(dx, dz, 0);

					// Log.Message(pe.Position + " to dest path is " + cachedDestCosts[pe]);
				}
				cachedDestX = destX;
				cachedDestZ = destZ;
			}

			foreach (Building_PortalGunPortalEntry entry in Portals)
			{
				int dx = Math.Abs(entry.Position.x - startX);
				int dz = Math.Abs(entry.Position.z - startZ);
				int entryCost = PortalPath.GetCost(dx, dz, 0, cardCost, diagCost);
				if (entryCost < bestCost)
				{
					foreach (Building_PortalGunPortalEntry exit in Portals)
					{
						if (entry == exit)
						{
							continue;
						}

						int totalCost = entryCost + DistanceLookup[entry][exit].GetCost(cardCost, diagCost) + cachedDestCosts[exit].GetCost(cardCost, diagCost);
						if (totalCost < bestCost)
						{
							bestCost = totalCost;
						}
					}
				}
			}

			return bestCost;
		}
	}
}
