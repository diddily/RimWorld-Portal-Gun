using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Portal_Gun.Toils
{
    public static class Toils_PortalGun
    {
        public static Toil ToilSetJobToUsePortalVerb(TargetIndex targetInd, Verb portalVerb)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                curJob.verbToUse = portalVerb;
            };
            return toil;
        }

        public static Toil GotoCastPosition(TargetIndex targetInd, float maxRangeFactor = 1f)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(targetInd).Thing;
                Pawn pawn = thing as Pawn;
                IntVec3 intVec;
                if (!CastPositionFinder.TryFindCastPosition(new CastPositionRequest
                {
                    caster = toil.actor,
                    target = thing,
                    verb = curJob.verbToUse,
                    maxRangeFromTarget = Mathf.Max(curJob.verbToUse.verbProps.range * maxRangeFactor, 1.42f),
                    wantCoverFromTarget = false
                }, out intVec))
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }
                toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
                actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, intVec);
            };
            toil.FailOnDespawnedOrNull(targetInd);
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        public static Toil CastVerb(TargetIndex targetInd, bool canHitNonTargetPawns = true)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Verb arg_45_0 = toil.actor.jobs.curJob.verbToUse;
                LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(targetInd);
                bool canHitNonTargetPawns2 = canHitNonTargetPawns;
                arg_45_0.TryStartCastOn(target, false, canHitNonTargetPawns2);
            };
            toil.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
            return toil;
        }
    }
}
