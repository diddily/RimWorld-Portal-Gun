using System;
using Verse;


namespace Portal_Gun.Comps
{
	public class CompProperties_PortalGunPortal : CompProperties
	{
		public GraphicData wallFrontGraphic;
		public GraphicData wallBackGraphic;
		public GraphicData floorFrontGraphic;
		public GraphicData floorBackGraphic;

		public CompProperties_PortalGunPortal()
		{
			this.compClass = typeof(Comp_PortalGunPortal);
		}
	}
}
