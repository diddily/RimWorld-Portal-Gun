﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using Portal_Gun.Items;

namespace Portal_Gun.Projectiles
{
    public class Projectile_PortalGun : Projectile
    {
        public Item_PortalGun launcherGun;

        protected override void Impact(Thing hitThing)
        {
            if (hitThing == null || (hitThing.def == ThingDefOf.Wall || hitThing.def.IsSmoothed))
            {
                Vector3 offset = (origin - ExactPosition);
                IntVec3 position = (hitThing != null ? hitThing.Position : Position);
                if (launcherGun.CreatePortal(this, offset, position, Map, hitThing != null))
                {
                    if (def.projectile.soundExplode != null)
                    {
                        if (hitThing != null)
                        {
                            def.projectile.soundExplode.PlayOneShot(new TargetInfo(hitThing));
                        }
                        else
                        {
                            def.projectile.soundExplode.PlayOneShot(new TargetInfo(Position, Map, false));
                        }
                    }
                }
            }
            base.Impact(hitThing);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Item_PortalGun>(ref launcherGun, "launcherGun", true);
        }

        public override void Tick()
        {
            if (launcherGun == null || launcherGun.Destroyed)
            {
                landed = true;
                this.Destroy(DestroyMode.Vanish);
            }
            base.Tick();
        }
    }
}