using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Portal_Gun.Items;

namespace Portal_Gun.Buildings
{
    public class Building_PortalGunPortalEntry : Building
    {
        public Building_PortalGunPortal linkedPortal;
        private CompFlickable compFlick;

        public bool IsConnected
        {
            get
            {
                return linkedPortal != null && linkedPortal.LinkedPortal != null && linkedPortal.LinkedPortal.linkedEntry != null;
            }
        }

        public bool IsConnectedLocal
        {
            get
            {
                return IsConnected && Map == linkedPortal.Map;
            }
        }

        public Building_PortalGunPortalEntry Exit
        {
            get
            {
                if (IsConnected)
                {
                    return linkedPortal.LinkedPortal.linkedEntry;
                }

                return null;
            }
        }

        public bool CanTraverse(Pawn pawn)
        {
            return IsConnected && !linkedPortal.IsForbidden(pawn);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compFlick = GetComp<CompFlickable>();
            ToggleLight(true);
            Utilities.Notify_PortalEntrySpawned(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Utilities.Notify_PortalEntryDeSpawned(this);
            base.DeSpawn(mode);
        }

        public void ToggleLight(bool on)
        {
            if (compFlick != null)
            {
                compFlick.SwitchIsOn = on;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref linkedPortal, "linkedPortal");
        }
    }
}