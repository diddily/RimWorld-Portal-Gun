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
                Props.wallFrontGraphic.GraphicColoredFor(Portal).Draw(frontDrawPos, parent.Rotation, parent, 0f);
                if (Portal.IsConnected)
                {
                    Props.wallBackGraphic.GraphicColoredFor(Portal).Draw(drawPos, parent.Rotation, parent, 0f);
                }
                else
                {
                    Props.wallBackGraphic.Graphic.GetColoredVersion(Props.wallFrontGraphic.Graphic.Shader, Color.black, Color.black).Draw(drawPos, parent.Rotation, parent, 0f);
                }
            }
            else
            {
                Props.floorFrontGraphic.GraphicColoredFor(Portal).Draw(frontDrawPos, parent.Rotation, parent, Portal.FloorRotation);
                if (Portal.IsConnected)
                {
                    Props.floorBackGraphic.GraphicColoredFor(Portal).Draw(drawPos, parent.Rotation, parent, Portal.FloorRotation);
                }
                else
                {
                    Props.floorBackGraphic.Graphic.GetColoredVersion(Props.wallFrontGraphic.Graphic.Shader, Color.black, Color.black).Draw(drawPos, parent.Rotation, parent, Portal.FloorRotation);
                }
            }
        }
    }
}
