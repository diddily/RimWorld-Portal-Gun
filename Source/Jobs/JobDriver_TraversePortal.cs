using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Portal_Gun.Jobs
{
	class JobDriver_TraversePortal : JobDriver
	{
		public bool finished = false;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		public void TryEnterNextPathCell()
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
						return;
					}
					Log.Message("nope");
					pawn.pather.StopDead();
					pawn.jobs.curDriver.Notify_PatherFailed();
					return;
				}
			}
			Building_Door building_Door2 = pawn.Map.thingGrid.ThingAt<Building_Door>(nextCell);
			Log.Message(pawn + " nextCell building2 = " + building);
			if (building_Door2 != null && building_Door2.SlowsPawns && !building_Door2.Open && building_Door2.PawnCanOpen(this.pawn))
			{
				Stance_Cooldown stance_Cooldown = new Stance_Cooldown(building_Door2.TicksToOpenNow, building_Door2, null);
				stance_Cooldown.neverAimWeapon = true;
				pawn.stances.SetStance(stance_Cooldown);
				building_Door2.StartManualOpenBy(pawn);
				building_Door2.CheckFriendlyTouched(pawn);
				return;
			}
			IntVec3 lastCell = pawn.Position;
			pawn.pather.nextCell = pawn.pather.curPath.ConsumeNextNode();
			pawn.Position = pawn.pather.nextCell;
			pawn.pather.nextCellCostTotal = job.count / 2;
			pawn.pather.nextCellCostLeft = job.count / 2;
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
			Building_Door building_Door3 = this.pawn.Map.thingGrid.ThingAt<Building_Door>(lastCell);
			if (building_Door3 != null && !pawn.HostileTo(building_Door3))
			{
				building_Door3.CheckFriendlyTouched(pawn);
				if (!building_Door3.BlockedOpenMomentary && !building_Door3.HoldOpen && building_Door3.SlowsPawns && building_Door3.PawnCanOpen(pawn))
				{
					building_Door3.StartManualCloseBy(pawn);
					return;
				}
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (TargetA != null && TargetB != null)
			{
				this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
				this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
				this.FailOn(() => pawn.Dead);

				yield return new Toil
				{
					defaultCompleteMode = ToilCompleteMode.Delay,
					defaultDuration = job.count / 2,
					handlingFacing = true,
					tickAction = delegate
					{
						pawn.rotationTracker.FaceTarget(pawn.CurJob.GetTarget(TargetIndex.A));
					}
				};
				yield return Toils_General.Do(delegate {

					if (pawn.pather.curPath.NodesLeftCount > 1)
					{
						//TryEnterNextPathCell();
					}
				});
				yield return new Toil
				{
					defaultCompleteMode = ToilCompleteMode.Delay,
					defaultDuration = job.count,
					handlingFacing = true,
					tickAction = delegate
					{
						pawn.rotationTracker.FaceTarget(pawn.CurJob.GetTarget(TargetIndex.B));
					}
				};
				yield return Toils_General.Do(delegate {
					finished = true;						//ended = true;
				});
			}
		}
	}
}
