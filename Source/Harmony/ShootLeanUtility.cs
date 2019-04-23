using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Buildings;

namespace Portal_Gun.Harmony
{
    [HarmonyPatch(typeof(ShootLeanUtility), "LeanShootingSourcesFromTo")]
    public class ShootLeanUtility_LeanShootingSourcesFromTo
    {
        static List<IntVec3> tmpList = new List<IntVec3>(20);
        public static void Postfix(IntVec3 shooterLoc, IntVec3 targetPos, Map map, List<IntVec3> listToFill)
        {
            tmpList.Clear();
            tmpList.Concat(shooterLoc.GetThingList(map).OfType<Building_PortalGunPortalEntry>().Where(pe => pe.IsConnected).Select(pe => pe.Exit.Position));
               
            foreach (IntVec3 pos in listToFill)
            {
                tmpList.Concat(pos.GetThingList(map).OfType<Building_PortalGunPortalEntry>().Where(pe => pe.IsConnected).Select(pe => pe.Exit.Position));
            }
            if (tmpList.Count > 0)
            {
                Log.Message("LeanShootingSourcesFromTo (" + shooterLoc + " -> " + targetPos + ") added: " + tmpList.Join(c => c.ToString(), ","));
            }
            listToFill.Concat(tmpList);
        }
    }

    [HarmonyPatch(typeof(ShootLeanUtility), "CalcShootableCellsOf")]
    public class ShootLeanUtility_CalcShootableCellsOf
    {
        static List<IntVec3> tmpList = new List<IntVec3>(20);
        public static void Postfix(List<IntVec3> outCells, Thing t)
        {
            tmpList.Clear();
            foreach (IntVec3 pos in outCells)
            {
                tmpList.Concat(pos.GetThingList(t.Map).OfType<Building_PortalGunPortalEntry>().Where(pe => pe.IsConnected).Select(pe => pe.Exit.Position));
            }

            if (tmpList.Count > 0)
            {
                Log.Message("CalcShootableCellsOf (" + t + ") added: " + tmpList.Join(c => c.ToString(), ","));
            }
            outCells.Concat(tmpList);
        }
    }
}
