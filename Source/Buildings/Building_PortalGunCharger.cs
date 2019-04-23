using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Portal_Gun.Comps;
using Portal_Gun.Items;

namespace Portal_Gun.Buildings
{
    public class Building_PortalGunCharger : Building
    {

        private CompPowerPlant_PortalGunCharger compPortalGunCharger => GetComp<CompPowerPlant_PortalGunCharger>();

        public override Graphic Graphic
        {
            get
            {
                if (compPortalGunCharger.IsCharging)
                {
                    return compPortalGunCharger.PG_Props.graphicOn.Graphic;
                }
                else
                {
                    return compPortalGunCharger.PG_Props.graphicOff.Graphic;
                }
            }
        }

    }
}