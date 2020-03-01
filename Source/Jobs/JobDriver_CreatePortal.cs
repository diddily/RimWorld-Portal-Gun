using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Portal_Gun.Jobs
{
	class JobDriver_CreatePortal : JobDriver
	{
		public bool finished = false;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (TargetA != null)
			{
				this.FailOnDespawnedOrNull(TargetIndex.A);
				this.FailOn(() => pawn.Dead);
				yield return Toils_Combat.GotoCastPosition(TargetIndex.A, false, 1f);
				yield return Toils_Combat.CastVerb(TargetIndex.A, true);
				yield return Toils_General.Do(delegate {
					finished = true;						//ended = true;
				});
			}
		}
	}
}
