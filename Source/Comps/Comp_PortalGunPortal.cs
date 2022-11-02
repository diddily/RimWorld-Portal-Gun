using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Portal_Gun.Buildings;

namespace Portal_Gun.Comps
{
	class Comp_PortalGunPortal : ThingComp
	{
		private Graphic cachedFrontGraphic;
		private Graphic cachedBackOnGraphic;
		private Graphic cachedBackOffGraphic;

		public CompProperties_PortalGunPortal Props => props as CompProperties_PortalGunPortal;
		public Building_PortalGunPortal Portal => parent as Building_PortalGunPortal;
		private static Color offColor = new Color(0, 0, 0, 1);
		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 drawPos = parent.DrawPos;
			Vector3 frontDrawPos = drawPos;
			frontDrawPos.y += 0.046875f;
			if (Portal.IsWall)
			{
				if (cachedFrontGraphic == null)
				{
					cachedFrontGraphic = Props.wallFrontGraphic.GraphicColoredFor(Portal);
				}
				cachedFrontGraphic.Draw(frontDrawPos, parent.Rotation, parent, 0f);
				if (Portal.IsConnected)
				{
					if (cachedBackOnGraphic == null)
					{
						cachedBackOnGraphic = Props.wallBackGraphic.GraphicColoredFor(Portal);
					}
					cachedBackOnGraphic.Draw(drawPos, parent.Rotation, parent, 0f);
				}
				else
				{
					if (cachedBackOffGraphic == null)
					{
						cachedBackOffGraphic = Props.wallBackGraphic.Graphic.GetColoredVersion(Props.wallFrontGraphic.Graphic.Shader, Color.black, Color.black);
					}
					cachedBackOffGraphic.Draw(drawPos, parent.Rotation, parent, 0f);
				}
				return;
			}

			if (cachedFrontGraphic == null)
			{
				cachedFrontGraphic = Props.floorFrontGraphic.GraphicColoredFor(Portal);
			}
			cachedFrontGraphic.Draw(frontDrawPos, parent.Rotation, parent, Portal.FloorRotation);
			if (Portal.IsConnected)
			{
				if (cachedBackOnGraphic == null)
				{
					cachedBackOnGraphic = Props.floorBackGraphic.GraphicColoredFor(Portal);
				}
				cachedBackOnGraphic.Draw(drawPos, parent.Rotation, parent, Portal.FloorRotation);
			}
			else
			{
				if (cachedBackOffGraphic == null)
				{
					cachedBackOffGraphic = Props.floorBackGraphic.Graphic.GetColoredVersion(Props.floorBackGraphic.Graphic.Shader, Color.black, Color.black);
				}
				cachedBackOffGraphic.Draw(drawPos, parent.Rotation, parent, Portal.FloorRotation);
			}
			
		}
	}
}
