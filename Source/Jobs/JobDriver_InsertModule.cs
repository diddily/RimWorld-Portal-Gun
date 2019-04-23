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
    class JobDriver_InsertModule : JobDriver
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
            //this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);
            
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);
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
                    PortalGun.AddModule(Module);
                    //Module.Destroy(DestroyMode.Vanish);
                }
            };
        }
    }
}
