using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using Portal_Gun.Buildings;
using Portal_Gun.Items;

namespace Portal_Gun.Comps
{
    public class Comp_PortalGunModule : ThingComp
    {
        public CompProperties_PortalGunModule Props => props as CompProperties_PortalGunModule;

        public float storedEnergy = 0.0f;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref storedEnergy, "storedEnergy");
        }

        /*public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            if (myPawn.skills.GetSkill(SkillDefOf.Crafting).TotallyDisabled)
            {
                yield return new FloatMenuOption("PG_InsertModule".Translate() + "(" + "SkillDisabled".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption("PG_InsertModule".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.CanReserve(parent, 1, -1, null, false))
            {
                yield return new FloatMenuOption("PG_InsertModule".Translate() + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                yield return new FloatMenuOption("PG_InsertModule".Translate() + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                FloatMenuOption useopt = new FloatMenuOption("PG_InsertModule".Translate(), delegate
                {
                    if (myPawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
                        TargetingParameters targetingParameters = new TargetingParameters
                        {
                            canTargetPawns = false,
                            canTargetBuildings = false,
                            mapObjectTargetsMustBeAutoAttackable = false,
                            neverTargetDoors = true,
                            validator = ((TargetInfo x) => x.Thing != null && (x.Thing.def == PG_DefOf.PG_PortalGun))
                        };
                        Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo target)
                        {
                            TryStartUseJob(myPawn, target);
                        }, myPawn, null, null);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return useopt;
            }

        }*/

        public void TryStartUseJob(Pawn user, LocalTargetInfo target)
        {
            if (user.skills.GetSkill(SkillDefOf.Crafting).TotallyDisabled)
            {
                return;
            }
            if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                return;
            }
            if (!user.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return;
            }
            Job job = new Job(PG_DefOf.PG_JobInsertModule, parent, target);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}
