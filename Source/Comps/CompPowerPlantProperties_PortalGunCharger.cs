using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Portal_Gun.Comps
{
	public class CompPowerPlantProperties_PortalGunCharger : CompProperties_Power
	{
		public GraphicData graphicOn;
		public GraphicData graphicOff;

		public CompPowerPlantProperties_PortalGunCharger()
		{
			compClass = typeof(Comp_PortalGun);
		}
	}
}
