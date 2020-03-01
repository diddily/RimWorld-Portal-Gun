using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Portal_Gun.Comps
{
	public class CompTargetEffect_AddModule : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (!user.IsColonistPlayerControlled)
			{
				return;
			}
			if (!user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
			{
				return;
			}
			Job job = new Job(PG_DefOf.PG_JobInsertModule, parent, target);
			job.count = 1;
			user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}
}
