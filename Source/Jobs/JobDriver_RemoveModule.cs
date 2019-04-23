using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Portal_Gun.Items;

namespace Portal_Gun.Jobs
{
    class JobDriver_RemoveModule : JobDriver
    {
        private const TargetIndex ModuleIndex = TargetIndex.A;

        private const TargetIndex PortalGunIndex = TargetIndex.B;

        private Thing Module
        {
            get
            {
                return job.targetA.Thing;
            }
        }

        private Item_PortalGun PortalGun
        {
            get
            {
                return (Item_PortalGun)job.targetB.Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(PortalGun, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            Toil install = Toils_General.Wait(60, TargetIndex.None);
            install.FailOnDespawnedOrNull(TargetIndex.B);
            install.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            install.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return install;
            yield return new Toil
            {
                initAction = delegate
                {
                    PortalGun.RemoveModule(Module, pawn);
                    GenPlace.TryPlaceThing(Module, pawn.Position, Map, ThingPlaceMode.Near, null, null);
                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(Module);
                    IntVec3 c;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(Module, pawn, Map, currentPriority, pawn.Faction, out c, true))
                    {
                        job.SetTarget(TargetIndex.C, c);
                    }
                    else
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);

            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);
        }
    }
}
