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
        public int surfaceLevel;
        public bool unique;
        public bool preventSolarFlare;

        public bool allowViolence;
        public bool allowWorldPortals;

        public CompProperties_PortalGunModule()
        {
            this.compClass = typeof(Comp_PortalGunModule);
        }
    }
}
