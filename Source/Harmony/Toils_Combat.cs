using Harmony;
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

namespace Portal_Gun.Harmony
{
    [HarmonyPatch(typeof(Toils_Combat), "GotoCastPosition")]
    public class Toils_Combat_GotoCastPosition
    {
        public static void Postfix(ref Toil __result, TargetIndex targetInd, bool closeIfDowned, float maxRangeFactor)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(targetInd).Thing;
                if (thing == null)
                {
                    thing = GenSpawn.Spawn(ThingMaker.MakeThing(PG_DefOf.PG_Dummy), curJob.GetTarget(targetInd).Cell, actor.Map);
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
                        thing.Destroy();
                        toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        return;
                    }
                    thing.Destroy();
                    toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
                    actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, intVec);
                }
                else
                {
                    Pawn pawn = thing as Pawn;
                    IntVec3 intVec;
                    if (!CastPositionFinder.TryFindCastPosition(new CastPositionRequest
                    {
                        caster = toil.actor,
                        target = thing,
                        verb = curJob.verbToUse,
                        maxRangeFromTarget = ((closeIfDowned && pawn != null && pawn.Downed) ? Mathf.Min(curJob.verbToUse.verbProps.range, (float)pawn.RaceProps.executionRange) : Mathf.Max(curJob.verbToUse.verbProps.range * maxRangeFactor, 1.42f)),
                        wantCoverFromTarget = false
                    }, out intVec))
                    {
                        toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        return;
                    }
                    toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
                    actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, intVec);
                }
            };

            toil.FailOnDespawnedOrNull(targetInd);
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            __result = toil;
        }

        /*public static float HitFactorFromShooter(Thing caster, float distance)
		{
			float accRating = (!(caster is Pawn)) ? caster.GetStatValue(StatDefOf.ShootingAccuracyTurret, true) : caster.GetStatValue(StatDefOf.ShootingAccuracyPawn, true);
			return ShotReport.HitFactorFromShooter(accRating, distance);
		}*/
    }
}
