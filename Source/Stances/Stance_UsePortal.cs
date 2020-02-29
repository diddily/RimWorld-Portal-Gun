using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Portal_Gun.Buildings;

namespace Portal_Gun.Stances
{
    public class Stance_UsePortal : Stance_Mobile
    {
        public int tickCount;
        public int tickTotal;
        public Vector3 curOffset = new Vector3(0f, 0f, 0f);
        private Vector3 entryOffset = new Vector3(0f, 0f, 0f);
        bool entryFloor;
        private Vector3 exitOffset = new Vector3(0f, 0f, 0f);
        bool exitFloor;
        Building_PortalGunPortalEntry portalEntry;
        Building_PortalGunPortalEntry portalExit;
        private static float wallScale = -0.5f;
        private static Vector3 floorOffset = new Vector3(0f, 0f, 0.25f);

        public Stance_UsePortal(int _tickTotal, Building_PortalGunPortalEntry _portalEntry, Building_PortalGunPortalEntry _portalExit)
        {
            tickCount = 0;
            tickTotal = Math.Max(4, _tickTotal);
            portalEntry = _portalEntry;
            portalExit = _portalExit;
            if (portalEntry.linkedPortal.IsWall)
            {
                entryOffset = portalEntry.linkedPortal.Rotation.FacingCell.ToVector3() * wallScale;
                entryFloor = false;
            }
            else
            {
                entryOffset = floorOffset;
                entryFloor = true;
            }

            if (portalExit.linkedPortal.IsWall)
            {
                exitOffset = portalExit.linkedPortal.Rotation.FacingCell.ToVector3() * wallScale;
                exitFloor = false;
            }
            else
            {
                exitOffset = floorOffset;
                exitFloor = true;
            }
        }

        public bool TryEnterNextPathCell()
        {
            Pawn pawn = Pawn;
            if (pawn != null && pawn.pather != null && pawn.pather.curPath != null && pawn.pather.curPath.NodesLeftCount > 1)
            {
                IntVec3 nextCell = pawn.pather.curPath.Peek(1);
                Building building = nextCell.GetEdifice(pawn.Map);
                Log.Message(pawn + " nextCell building = " + building);
                if (building != null && building.BlocksPawn(pawn))
                {
                    Building_Door building_Door = building as Building_Door;
                    if (building_Door == null || !building_Door.FreePassage)
                    {
                        Log.Message((pawn.CurJob != null && pawn.CurJob.canBash) + " || " + (pawn.jobs.jobQueue.Count > 0 && pawn.jobs.jobQueue.Peek().job.canBash));
                        if ((pawn.CurJob != null && pawn.CurJob.canBash) || (pawn.jobs.jobQueue.Count > 0 && pawn.jobs.jobQueue.Peek().job.canBash) || pawn.HostileTo(building))
                        {
                            Job job = new Job(JobDefOf.AttackMelee, building);
                            job.expiryInterval = 300;
                            pawn.jobs.StartJob(job, JobCondition.Incompletable, null, false, true, null, null, false);
                            return false;
                        }
                        Log.Message("nope");
                        pawn.pather.StopDead();
                        pawn.jobs.curDriver.Notify_PatherFailed();
                        return false;
                    }
                }
                Building_Door building_Door2 = pawn.Map.thingGrid.ThingAt<Building_Door>(nextCell);
                Log.Message(pawn + " nextCell building2 = " + building);
                if (building_Door2 != null && building_Door2.SlowsPawns && !building_Door2.Open && building_Door2.PawnCanOpen(pawn))
                {
                    Stance_Cooldown stance_Cooldown = new Stance_Cooldown(building_Door2.TicksToOpenNow, building_Door2, null);
                    stance_Cooldown.neverAimWeapon = true;
                    pawn.stances.SetStance(stance_Cooldown);
                    building_Door2.StartManualOpenBy(pawn);
                    building_Door2.CheckFriendlyTouched(pawn);
                    return false;
                }
                IntVec3 lastCell = pawn.Position;
                pawn.pather.nextCell = pawn.pather.curPath.ConsumeNextNode();
                pawn.Position = pawn.pather.nextCell;
                pawn.pather.nextCellCostTotal = tickCount / 2 + 1;
                pawn.pather.nextCellCostLeft = pawn.pather.nextCellCostTotal;
                pawn.Drawer.tweener.ResetTweenedPosToRoot();

                /*if (pawn.RaceProps.Humanlike)
                {
                    pawn.pather.cellsUntilClamor--;
                    if (this.cellsUntilClamor <= 0)
                    {
                        GenClamor.DoClamor(this.pawn, 7f, ClamorDefOf.Movement);
                        this.cellsUntilClamor = 12;
                    }
                }*/
                pawn.filth.Notify_EnteredNewCell();
                if (pawn.BodySize > 0.9f)
                {
                    pawn.Map.snowGrid.AddDepth(pawn.Position, -0.001f);
                }
                Building_Door building_Door3 = pawn.Map.thingGrid.ThingAt<Building_Door>(lastCell);
                if (building_Door3 != null && !pawn.HostileTo(building_Door3))
                {
                    building_Door3.CheckFriendlyTouched(pawn);
                    if (!building_Door3.BlockedOpenMomentary && !building_Door3.HoldOpen && building_Door3.SlowsPawns && building_Door3.PawnCanOpen(pawn))
                    {
                        building_Door3.StartManualCloseBy(pawn);
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        public void ApplyFraction(float fraction, bool isFloor, Vector3 offset)
        {
            if (isFloor && fraction >= 0.5f)
            {
                fraction = 1.0f - fraction;
            }
            curOffset = offset * fraction;
        }

        public override void StanceTick()
        {
            if (tickCount >= tickTotal)
            {
                stanceTracker.SetStance(new Stance_Mobile());
                return;
            }

            int midway = tickTotal / 2;
            if (tickCount >= midway && (portalEntry.DestroyedOrNull() || !portalEntry.Spawned || portalExit.DestroyedOrNull() || !portalExit.Spawned))
            {
                stanceTracker.SetStance(new Stance_Mobile());
                return;
            }

            if (tickCount == midway)
            {
                if (!TryEnterNextPathCell())
                {
                    if (stanceTracker.curStance == this)
                    {
                        stanceTracker.SetStance(new Stance_Mobile());
                    }
                    return;
                }
            }

            if (tickCount < midway)
            {
                ApplyFraction((float)tickCount / (midway-1), entryFloor, entryOffset);
            }
            else
            {
                ApplyFraction((float)(tickTotal - tickCount) / midway, exitFloor, exitOffset);
            }

            tickCount++;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref tickCount, "tickCount");
            Scribe_Values.Look<int>(ref tickTotal, "tickTotal");
            Scribe_Values.Look<bool>(ref entryFloor, "entryFloor");
            Scribe_Values.Look<bool>(ref exitFloor, "exitFloor");
            Scribe_Values.Look<Vector3>(ref curOffset, "curOffset");
            Scribe_Values.Look<Vector3>(ref entryOffset, "entryOffset");
            Scribe_Values.Look<Vector3>(ref exitOffset, "exitOffset");
            Scribe_References.Look(ref portalEntry, "portalEntry");
            Scribe_References.Look(ref portalExit, "portalExit");
        }
    }
}
