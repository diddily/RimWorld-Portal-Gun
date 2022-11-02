using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Portal_Gun.Comps
{
	public class CompProperties_PortalGun : CompProperties_ChangeableProjectile
	{
		public GraphicData secondaryGraphic;
		public GraphicData secondaryGLaDOSGraphic;
		public GraphicData primaryGraphic;
		public GraphicData primaryGLaDOSGraphic;
		public GraphicData offGraphic;
		public GraphicData offGLaDOSGraphic;
		public SoundDef outOfPowerSound;
		public float portalPowerDraw;
		public float basePowerDraw;
		public float rechargePowerDraw;
		public List<VerbProperties> GLaDOSVerbs;
		public List<VerbProperties> defaultVerbs;

		public CompProperties_PortalGun()
		{
			compClass = typeof(Comp_PortalGun);
		}
	}
}
