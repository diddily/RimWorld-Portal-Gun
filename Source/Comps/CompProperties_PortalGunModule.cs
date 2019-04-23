using System;
using Verse;


namespace Portal_Gun.Comps
{
    public class CompProperties_PortalGunModule : CompProperties
    {
        public float rangeMult;
        public float inaccuracyFactor;
        public float powerOutput;
        public float powerEfficiency;
        public float powerStorage;

        bool preventSolarFlare;
        bool allowViolence;
        bool allowWorldPortals;
        bool unique;

        public CompProperties_PortalGunModule()
        {
            this.compClass = typeof(Comp_PortalGunModule);
        }
    }
}
