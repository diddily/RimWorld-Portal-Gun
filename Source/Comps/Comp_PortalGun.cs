using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Portal_Gun.Buildings;
using Portal_Gun.Items;

namespace Portal_Gun.Comps
{
    public class Comp_PortalGun : CompChangeableProjectile
    {
        public CompProperties_PortalGun PG_Props => props as CompProperties_PortalGun;

        public bool IsSecondary
        {
            get
            {
                return LoadedShell == PG_DefOf.PG_PortalGun_ShellSecondary;
            }
            set
            {
                LoadShell((value ? PG_DefOf.PG_PortalGun_ShellSecondary : PG_DefOf.PG_PortalGun_ShellPrimary), 1);
            }
        }

        public Comp_PortalGun()
        { 
            if (!Loaded)
            {
                //LoadShell(PG_DefOf.PG_PortalGun_ShellPrimary, 1);
            }
        }

        public override void Notify_ProjectileLaunched()
        {
            // Don't change anything
        }

        /*public override string CompInspectStringExtra()
        {
            Item_PortalGun pg = parent as Item_PortalGun;

            string str;
            if (pg.HasPower)
            {
                if (pg.PowerCharging > 0)
                {
                    str = "Charging: " + (pg.PowerPercent * 100).ToString("#####0") + "%";
                }
                else if (pg.PowerCharging < 0)
                {
                    str = "Discharging: " + (pg.PowerPercent * 100).ToString("#####0") + "%";
                }
                else
                {
                    str = "Fully charged";
                }
            }
            else
            {
                str = "No power";
            }
            return str + "\n" + base.CompInspectStringExtra();
        }*/
        
    }
}