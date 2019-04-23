using Harmony;
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
using Portal_Gun.Jobs;
using Portal_Gun.Stances;

namespace Portal_Gun.Harmony
{
    [HarmonyPatch(typeof(Pawn_PathFollower), "SetupMoveIntoNextCell")]
    public class PathFinder_SetupMoveIntoNextCell
    {
        public static bool Prefix(ref Pawn_PathFollower __instance, ref Pawn ___pawn)
        {
            Pawn_PathFollower instance = __instance;
            if (__instance.curPath.NodesLeftCount > 1)
            {
                Pawn pawn = ___pawn;
                Building_PortalGunPortalEntry portalEntry = pawn.Position.GetThingList(pawn.Map).OfType<Building_PortalGunPortalEntry>().Where(p => p.IsConnected && p.Exit.Position == instance.curPath.Peek(1)).FirstOrDefault();
                if (portalEntry != null)
                {
                    /*Job job = new Job(PG_DefOf.PG_JobTraversePortal, portalEntry, portalEntry.linkedPortal.LinkedPortal.linkedEntry)
                    {
                        count = CostToMoveIntoPortal(pawn, portalEntry.linkedPortal.Position),
                    };
                    pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true);
                    __instance.nextCellCostTotal = job.count + 1;
                    __instance.nextCellCostLeft = __instance.nextCellCostTotal;
                    Building_Door building_Door = pawn.Map.thingGrid.ThingAt<Building_Door>(instance.curPath.Peek(1));
                    if (building_Door != null)
                    {
                        building_Door.Notify_PawnApproaching(pawn, job.count);
                    }*/
                    int cost = CostToMoveIntoPortal(pawn, portalEntry.linkedPortal.Position);
                    Stance_UsePortal stancePortal = new Stance_UsePortal(cost, portalEntry, portalEntry.Exit);
                    pawn.stances.SetStance(stancePortal);
                    __instance.nextCellCostTotal = cost / 2 + 1;
                    __instance.nextCellCostLeft = __instance.nextCellCostTotal;
                    return false;
                }

              
            }

            return true;
        }

        private static int CostToMoveIntoPortal(Pawn pawn, IntVec3 c)
        {
            int num;
            if (c.x == pawn.Position.x || c.z == pawn.Position.z)
            {
                num = pawn.TicksPerMoveCardinal;
            }
            else
            {
                num = pawn.TicksPerMoveDiagonal;
            }

            num += 5;

            if (pawn.CurJob != null)
            {
                Pawn locomotionUrgencySameAs = pawn.jobs.curDriver.locomotionUrgencySameAs;
                if (locomotionUrgencySameAs != null && locomotionUrgencySameAs != pawn && locomotionUrgencySameAs.Spawned)
                {
                    int num2 = CostToMoveIntoPortal(locomotionUrgencySameAs, c);
                    if (num < num2)
                    {
                        num = num2;
                    }
                }
                else
                {
                    switch (pawn.jobs.curJob.locomotionUrgency)
                    {
                        case LocomotionUrgency.Amble:
                            num *= 3;
                            if (num < 60)
                            {
                                num = 60;
                            }
                            break;
                        case LocomotionUrgency.Walk:
                            num *= 2;
                            if (num < 50)
                            {
                                num = 50;
                            }
                            break;
                        case LocomotionUrgency.Jog:
                            //num = num;
                            break;
                        case LocomotionUrgency.Sprint:
                            num = Mathf.RoundToInt((float)num * 0.75f);
                            break;
                    }
                }
            }
            return Mathf.Max(num, 4);
        }
    }
}
