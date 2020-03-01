using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Portal_Gun.Items;

namespace Portal_Gun.Comps
{
	public class CompTargetable_PortalGun : CompTargetable
	{
		protected override bool PlayerChoosesTarget
		{
			get
			{
				return true;
			}
		}

		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = ((TargetInfo x) => x.Thing != null && x.Thing is Item_PortalGun && ((Item_PortalGun) x.Thing).CanAcceptModule(this.parent))
			};
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
		}
	}
}
