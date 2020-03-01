using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Portal_Gun
{
	[DefOf]
	public class PG_DefOf 
	{
		public static ThingDef PG_Dummy;
		public static ThingDef PG_PortalGun;
		public static ThingDef PG_WallPortal;
		public static ThingDef PG_FloorPortal;
		public static ThingDef PG_PortalEntry;

		public static ThingDef PG_GLaDOSModule;
		public static ThingDef PG_ViolenceCoreModule;
		public static ThingDef PG_AdventureCoreModule;

		public static ThingDef PG_PortalGun_ShellPrimary;
		public static ThingDef PG_PortalGun_ShellSecondary;
		public static ThingDef PG_PortalGun_BulletPrimary;
		public static ThingDef PG_PortalGun_BulletSecondary;

		public static JobDef PG_JobTraversePortal;
		public static JobDef PG_JobInsertModule;
		public static JobDef PG_JobRemoveModule;

		public static ResearchProjectDef PG_IntermediateModulesResearch;
		public static ResearchProjectDef PG_AdvancedModulesResearch;

		public static SoundDef PG_ChargeLoop;
		public static SoundDef PG_PortalGunOutOfPower;
	}
}
